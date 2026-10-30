using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Environment;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Mounts;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// Real physics check of prop collision (Docs/Collision.md): a tree, a rock (with the stale
    /// half-size collider Main.unity's Rock_* had) and the Workbench (trigger only) are made solid by
    /// <see cref="SolidPropBinder"/>, then a hero-sized CharacterController (r 0.35, h 2, step 0.3 —
    /// Main.unity's Player) is driven into each with cc.Move at run speed, at the fastest mounted
    /// gallop and in one long hitch frame. It must never overlap the prop nor end up past it head-on,
    /// and a slightly off-centre approach must slide around it and carry on.
    /// </summary>
    public class SolidPropCollisionTests
    {
        private const float HeroRadius = 0.35f;
        private const float HeroHeight = 2f;
        private const float Frame = 1f / 60f;

        private static readonly float RunSpeed = MountRules.HeroRunSpeed;
        private static readonly float GallopSpeed = MountRules.HeroRunSpeed * MountRules.MaxMoveMultiplier;

        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object o in _created)
            {
                if (o != null)
                {
                    Object.Destroy(o);
                }
            }

            _created.Clear();
        }

        [UnityTest]
        public IEnumerator Tree_Rock_Workbench_BlockAtRunAndGallop_AndCanBeWalkedAround()
        {
            var ground = Track(GameObject.CreatePrimitive(PrimitiveType.Plane));
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(12f, 1f, 4f);
            ground.transform.position = new Vector3(20f, 0f, 0f);

            var props = new[]
            {
                MakeTree(new Vector3(0f, 0f, 0f)),
                MakeRock(new Vector3(20f, 0.25f, 0f)),
                MakeWorkbench(new Vector3(40f, 0.5f, 0f)),
            };

            foreach (GameObject prop in props)
            {
                Assert.IsTrue(SolidPropBinder.Bind(prop), $"{prop.name} should be made solid");
                Assert.IsNotNull(FindSolid(prop), $"{prop.name} has a solid collider");
            }

            Physics.SyncTransforms();
            yield return null;

            foreach (GameObject prop in props)
            {
                float x = prop.transform.position.x;
                bool flatFace = prop.name == "Workbench";
                // A dead-centre push on the trunk/bench face has no tangent: the hero must stay in
                // front. The irregular rock may deflect it sideways (that's sliding, not tunnelling),
                // so for it only "never inside" is asserted.
                bool mustStayInFront = prop.name != "Rock_Test";

                foreach (float speed in new[] { RunSpeed, GallopSpeed })
                {
                    // Head-on: blocked in front, never inside.
                    CharacterController hero = MakeHero(new Vector3(x, 0.05f, -5f));
                    bool touched = Drive(hero, prop, Vector3.forward, speed, 2.5f, Frame);
                    Assert.IsTrue(touched, $"{prop.name} @ {speed:0.0} m/s: hero should have hit it");
                    if (mustStayInFront)
                    {
                        Assert.Less(hero.transform.position.z, prop.transform.position.z, $"{prop.name} @ {speed:0.0} m/s: walked through head-on");
                    }

                    Retire(hero);

                    // Off-centre / diagonal: slides along and gets past.
                    // (bench: diagonal along its flat face; rock: well off-centre, its hull is faceted).
                    Vector3 direction = flatFace ? new Vector3(0.6f, 0f, 1f).normalized
                        : mustStayInFront ? new Vector3(0.12f, 0f, 1f).normalized : new Vector3(0.25f, 0f, 1f).normalized;
                    hero = MakeHero(new Vector3(x + (mustStayInFront ? 0.15f : 0.5f), 0.05f, -5f));
                    Drive(hero, prop, direction, speed, 4f, Frame);
                    Assert.Greater(hero.transform.position.z, prop.transform.position.z + 1f, $"{prop.name} @ {speed:0.0} m/s: should slide around, not get stuck");
                    Retire(hero);
                }

                // One long hitch frame (0.25 s at gallop = 3.4 m in a single Move): the sweep still stops it.
                CharacterController hitch = MakeHero(new Vector3(x, 0.05f, -2.5f));
                Drive(hitch, prop, Vector3.forward, GallopSpeed, 0.25f, 0.25f);
                if (mustStayInFront)
                {
                    Assert.Less(hitch.transform.position.z, prop.transform.position.z, $"{prop.name}: tunnelled through in a hitch frame");
                }

                Retire(hitch);
                yield return null;
            }
        }

        /// <summary>Moves the hero like PlayerMotor (cc.Move, gravity stick); fails on any overlap with the prop. Returns whether a side hit happened.</summary>
        private static bool Drive(CharacterController hero, GameObject prop, Vector3 direction, float speed, float seconds, float dt)
        {
            bool touched = false;
            var overlaps = new Collider[16];
            for (float t = 0f; t < seconds; t += dt)
            {
                CollisionFlags flags = hero.Move(direction * (speed * dt) + Vector3.down * (2f * dt));
                touched |= (flags & CollisionFlags.Sides) != 0;

                Vector3 p = hero.transform.position;
                float r = HeroRadius * 0.85f; // inside the contact offset skin
                int count = Physics.OverlapCapsuleNonAlloc(p + Vector3.up * HeroRadius, p + Vector3.up * (HeroHeight - HeroRadius), r, overlaps, ~0, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count; i++)
                {
                    Assert.IsFalse(overlaps[i].transform.IsChildOf(prop.transform), $"hero overlaps {prop.name} at {p}");
                }
            }

            return touched;
        }

        /// <summary>Deactivates at once (Destroy is end-of-frame: a lingering CharacterController would block the next run).</summary>
        private static void Retire(CharacterController hero)
        {
            hero.gameObject.SetActive(false);
            Object.Destroy(hero.gameObject);
        }

        private static Collider FindSolid(GameObject prop)
        {
            foreach (Collider c in prop.GetComponentsInChildren<Collider>())
            {
                if (!c.isTrigger)
                {
                    return c;
                }
            }

            return null;
        }

        private CharacterController MakeHero(Vector3 position)
        {
            var go = Track(new GameObject("TestHero"));
            go.transform.position = position;
            var cc = go.AddComponent<CharacterController>();
            cc.radius = HeroRadius;
            cc.height = HeroHeight;
            cc.center = new Vector3(0f, HeroHeight * 0.5f, 0f);
            cc.stepOffset = 0.3f;
            cc.slopeLimit = 45f;
            cc.skinWidth = 0.08f;
            Physics.SyncTransforms();
            return cc;
        }

        private GameObject MakeTree(Vector3 position)
        {
            var root = Track(new GameObject("Tree_Test_0"));
            var lod0 = new GameObject("LOD0");
            lod0.transform.SetParent(root.transform, false);

            var points = new List<Vector3>();
            for (int ring = 0; ring <= 22; ring++)
            {
                for (int s = 0; s < 16; s++)
                {
                    float a = s * Mathf.PI * 2f / 16f;
                    points.Add(new Vector3(Mathf.Cos(a) * 0.25f, ring * 0.1f, Mathf.Sin(a) * 0.25f));
                }
            }

            for (int lat = 1; lat < 12; lat++)
            {
                float phi = lat * Mathf.PI / 12f;
                for (int s = 0; s < 16; s++)
                {
                    float a = s * Mathf.PI * 2f / 16f;
                    points.Add(new Vector3(0f, 3.2f, 0f) + new Vector3(Mathf.Sin(phi) * Mathf.Cos(a), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(a)) * 1.6f);
                }
            }

            var mesh = Track(new Mesh { name = "TestTree" });
            mesh.SetVertices(points);
            var indices = new int[points.Count];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }

            mesh.SetIndices(indices, MeshTopology.Points, 0);
            mesh.RecalculateBounds();
            lod0.AddComponent<MeshFilter>().sharedMesh = mesh;
            Renderer renderer = lod0.AddComponent<MeshRenderer>();
            root.AddComponent<LODGroup>().SetLODs(new[] { new LOD(0.1f, new[] { renderer }) });
            root.transform.position = position;
            return root;
        }

        private GameObject MakeRock(Vector3 position)
        {
            var rock = Track(new GameObject("Rock_Test"));
            Mesh mesh = Track(ProceduralRockGenerator.BuildRockMesh(1001, 2, 0.35f, 1f));
            rock.AddComponent<MeshFilter>().sharedMesh = mesh;
            rock.AddComponent<MeshRenderer>();
            rock.transform.position = position;
            rock.transform.localScale = new Vector3(0.85f, 0.6f, 0.85f);

            // Main.unity's Rock_* bug: a collider at half the visual size.
            var stale = rock.AddComponent<SphereCollider>();
            stale.radius = 0.5f;
            return rock;
        }

        private GameObject MakeWorkbench(Vector3 position)
        {
            GameObject bench = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            bench.name = "Workbench";
            bench.transform.position = position;
            bench.transform.localScale = new Vector3(2f, 1f, 1f);
            var trigger = bench.GetComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2f, 2f, 3f);
            return bench;
        }

        private T Track<T>(T o) where T : Object
        {
            _created.Add(o);
            return o;
        }
    }
}
