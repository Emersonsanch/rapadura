using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.NPC.Visuals;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// The procedural NPC figures (Docs/NPCs.md): every named resident of the bible, every city resident and every
    /// story villager has its own recipe; each look stays inside the mobile triangle budget, is deterministic and has a
    /// silhouette of its own; the skinned mesh is bound to the five bones; the idle maths stays bounded.
    /// </summary>
    public class NpcVisualsTests
    {
        /// <summary>The cast of Docs/WorldBible_Maniqueismo.md §3.1 (old ids) plus Vó Edna, Patig, Adda and the Answer.</summary>
        private static readonly string[] BibleCast =
        {
            "cida", "severino", "damiao", "fatima", "lazaro", "nenem", "bento", "tonico_haras", "hunter", "rosa", "bene",
            "padwaxtag", "malaquias", "fe", "vava", "socorro", "tiao", "tonho", "graca", "quinzinho", "benedita", "candinho",
            "fia", "zezinho", "vo_edna",
        };

        private static readonly string[] SandboxObjects =
        {
            "Npc_Shopkeeper", "Npc_Villager", "Npc_GatheringPoint", "Npc_HermitMystic", "Npc_RuinsArchivist", "Npc_WindtrailScholar", "Npc_TempleAcolyte",
        };

        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object o in _created)
            {
                if (o != null)
                {
                    Object.DestroyImmediate(o);
                }
            }

            _created.Clear();
        }

        [Test]
        public void EveryNamedNpcOfTheBible_HasItsOwnRecipe()
        {
            foreach (string id in BibleCast)
            {
                Assert.IsNotNull(NpcRecipes.Find(id), $"'{id}' has no recipe.");
            }

            Assert.GreaterOrEqual(NpcRecipes.Named.Count, 30);
            Assert.AreEqual(NpcRecipes.Named.Count, NpcRecipes.Named.Select(r => r.Id).Distinct().Count(), "Recipe ids must be unique.");
            Assert.AreEqual(BibleCast.Length, BibleCast.Select(NpcRecipes.Find).Distinct().Count(), "Two people of the cast share a look.");
        }

        [Test]
        public void EveryCityResident_AndTheCaravaneer_ResolveToANamedRecipe()
        {
            foreach (CitySpec city in CityCatalog.Cities)
            {
                foreach (CityResidentSpec resident in city.Residents)
                {
                    Assert.IsNotNull(NpcRecipes.Find(resident.ResidentId), $"{city.CityId}/{resident.ResidentId} has no recipe.");
                }
            }

            Assert.AreEqual("ammo", NpcRecipes.Find(CityCatalog.Mascate.ResidentId).Id);
        }

        [Test]
        public void EveryStoryVillager_AndSandboxNpc_HasALook()
        {
            foreach (VillageNpcSpec npc in VillageNpcCatalog.Npcs)
            {
                Assert.IsNotNull(NpcRecipes.Find(npc.NpcKey), $"{npc.NpcKey} has no recipe.");
            }

            foreach (string name in SandboxObjects)
            {
                string id = NpcSceneDresser.IdFromObjectName(name);
                NpcRecipe recipe = NpcRecipes.Resolve(id);
                Assert.IsNotNull(recipe, name);
                if (name != "Npc_Villager")
                {
                    Assert.IsNotNull(NpcRecipes.Find(id), $"{name} → '{id}' should be a named NPC.");
                }
            }

            Assert.AreEqual("hermit_mystic", NpcSceneDresser.IdFromObjectName("Npc_HermitMystic"));
            Assert.AreEqual("windtrail_scholar", NpcSceneDresser.IdFromObjectName("Npc_WindtrailScholar"));
            Assert.AreEqual("zaku", NpcRecipes.Find(NpcSceneDresser.IdFromObjectName("Npc_Shopkeeper")).Id);
        }

        [Test]
        public void EveryRecipe_TellsItsStory()
        {
            foreach (NpcRecipe recipe in NpcRecipes.Named)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(recipe.Story), $"{recipe.Id} has no story line (Docs/NPCs.md).");
                Assert.Greater(recipe.Props.Length, 0, $"{recipe.Id}: the role should read from a prop.");
            }
        }

        private static IEnumerable<NpcRecipe> AllLooks()
        {
            foreach (NpcRecipe recipe in NpcRecipes.Named)
            {
                yield return recipe;
            }

            foreach (BiomeType region in new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves })
            {
                for (int i = 0; i < 25; i++)
                {
                    yield return NpcRecipes.Villager("test_" + i, region);
                }
            }
        }

        [Test]
        public void EveryLook_StaysInsideTheMobileTriangleBudget()
        {
            foreach (NpcRecipe recipe in AllLooks())
            {
                int tris = NpcModelBuilder.CountTriangles(recipe);
                Assert.LessOrEqual(tris, 1500, $"{recipe.Id} is too heavy ({tris} tris).");
                Assert.GreaterOrEqual(tris, 350, $"{recipe.Id} is suspiciously empty ({tris} tris).");
            }
        }

        [Test]
        public void NamedNpcs_HaveDistinctSilhouettes()
        {
            var seen = new Dictionary<string, string>();
            foreach (NpcRecipe recipe in NpcRecipes.Named)
            {
                string key = recipe.SilhouetteKey;
                Assert.IsFalse(seen.ContainsKey(key), $"{recipe.Id} has the same silhouette as {(seen.TryGetValue(key, out string other) ? other : "?")}.");
                seen[key] = recipe.Id;
            }
        }

        [Test]
        public void Villagers_AreDeterministic_AndVaried()
        {
            NpcRecipe a = NpcRecipes.Villager("lot_7", BiomeType.Desert);
            NpcRecipe b = NpcRecipes.Villager("lot_7", BiomeType.Desert);
            Assert.AreEqual(a.SilhouetteKey + a.Main + a.Skin, b.SilhouetteKey + b.Main + b.Skin);

            var keys = new HashSet<string>();
            for (int i = 0; i < 20; i++)
            {
                NpcRecipe v = NpcRecipes.Villager("lot_" + i, BiomeType.Forest);
                keys.Add(v.SilhouetteKey + v.Main);
            }

            Assert.Greater(keys.Count, 12, "Generic villagers all look alike.");
            Assert.AreEqual("mariam", NpcRecipes.Resolve("cida", BiomeType.Desert).Id, "A named resident never falls back to a villager.");
        }

        [Test]
        public void BuiltMesh_IsSkinnedToFiveBones_OnPaletteCells()
        {
            NpcModel model = NpcModelBuilder.Build(NpcRecipes.Find("adamas"));
            _created.Add(model.Mesh);
            Mesh mesh = model.Mesh;
            Assert.AreEqual(NpcBones.Count, mesh.bindposes.Length);
            Assert.AreEqual(mesh.vertexCount, mesh.boneWeights.Length);
            var used = new HashSet<int>();
            foreach (BoneWeight w in mesh.boneWeights)
            {
                Assert.That(w.boneIndex0, Is.InRange(0, NpcBones.Count - 1));
                Assert.AreEqual(1f, w.weight0, 1e-5f);
                used.Add(w.boneIndex0);
            }

            Assert.AreEqual(NpcBones.Count, used.Count, "Every bone (root, body, head, both arms) carries geometry.");
            var uvs = new List<Vector2>();
            mesh.GetUVs(0, uvs);
            Assert.IsTrue(uvs.All(uv => uv.x > 0f && uv.x < 1f && uv.y > 0f && uv.y < 1f));
            Assert.IsTrue(uvs.Any(uv => NpcPalette.SwatchAt(uv) == NpcSwatch.Ember), "The hammer's ember should be on the mesh.");

            Assert.Greater(mesh.bounds.size.y, 1.6f);
            Assert.Less(mesh.bounds.size.y, 2.4f);
            Assert.AreEqual(0f, mesh.bounds.min.y, 0.02f, "Feet on the ground (y = 0).");
        }

        [Test]
        public void Ages_ReadFromTheirHeight()
        {
            float child = NpcModelBuilder.Build(NpcRecipes.Find("gabryab")).Height;
            float elder = NpcModelBuilder.Build(NpcRecipes.Find("mihrshah")).Height;
            float adult = NpcModelBuilder.Build(NpcRecipes.Find("omoforo")).Height;
            Assert.Less(child, elder);
            Assert.Less(elder, adult);
        }

        [Test]
        public void Figure_HasOneSkinnedRenderer_WithTheSharedMaterial()
        {
            var parent = new GameObject("npc_test");
            _created.Add(parent);
            NpcFigure figure = NpcFigure.Create(parent.transform, "parthenos", BiomeType.Caves, Vector3.zero);
            Assert.AreEqual(NpcFigure.ObjectName, figure.gameObject.name);
            Assert.AreEqual(1, parent.GetComponentsInChildren<Renderer>(true).Length);
            Assert.AreSame(NpcPalette.SharedMaterial, figure.Renderer.sharedMaterial);
            Assert.AreEqual(NpcBones.Count, figure.Renderer.bones.Length);

            NpcFigure other = NpcFigure.Create(parent.transform, "benedita", BiomeType.Caves, Vector3.right);
            Assert.AreSame(figure.Renderer.sharedMesh, other.Renderer.sharedMesh, "Same person, same cached mesh.");

            // The head turns toward someone standing to the figure's right, within limits.
            figure.Animate(1f, figure.transform.position + new Vector3(3f, 0f, 1f), 10f);
            float yaw = Mathf.DeltaAngle(0f, figure.Bone(NpcBones.Head).localEulerAngles.y);
            Assert.Greater(yaw, 20f);
            Assert.LessOrEqual(yaw, NpcIdleMath.MaxHeadYaw + 0.01f);
        }

        [Test]
        public void IdleMath_IsBounded()
        {
            Assert.AreEqual(0f, NpcIdleMath.LookAngles(new Vector3(0f, 0f, -3f)).x, "Nobody looks straight behind.");
            Vector2 side = NpcIdleMath.LookAngles(new Vector3(10f, 0f, 0.5f));
            Assert.AreEqual(NpcIdleMath.MaxHeadYaw, side.x, 1e-3f);
            Assert.Less(NpcIdleMath.LookAngles(new Vector3(0f, 2f, 2f)).y, 0f, "Look up at a taller target.");

            foreach (NpcGesture gesture in System.Enum.GetValues(typeof(NpcGesture)))
            {
                for (float t = 0f; t < 20f; t += 0.37f)
                {
                    NpcPoseOffsets o = NpcIdleMath.Gesture(gesture, t);
                    Assert.That(Mathf.Abs(o.ArmL.x) + Mathf.Abs(o.ArmR.x), Is.LessThan(330f), gesture.ToString());
                    Assert.That(Mathf.Abs(o.HeadYaw), Is.LessThanOrEqualTo(40f), gesture.ToString());
                    Assert.That(o.BodyBob, Is.InRange(-0.05f, 0.05f), gesture.ToString());
                    Assert.That(o.LookWeight, Is.InRange(0f, 1f), gesture.ToString());
                }

                float p = NpcIdleMath.Pulse(1.3f, 4f, 1f, 1f);
                Assert.That(p, Is.InRange(0f, 1f));
            }
        }
    }
}
