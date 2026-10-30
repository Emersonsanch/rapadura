using UnityEngine;

namespace Rapadura.Gameplay.NPC.Visuals
{
    /// <summary>
    /// Role props of the NPC figures. Hand props are built in posed model space and bound to the arm bone (they follow
    /// the idle gesture); worn props are bound to the body; nothing is a separate object.
    /// </summary>
    public static partial class NpcModelBuilder
    {
        private static void BuildProps(NpcMeshBuilder b, ref Rig rig)
        {
            foreach (NpcProp prop in rig.R.Props)
            {
                BuildProp(b, ref rig, prop);
            }
        }

        private static void InHand(NpcMeshBuilder b, bool right)
        {
            b.Bone = right ? NpcBones.ArmR : NpcBones.ArmL;
            b.Matrix = Matrix4x4.identity;
        }

        private static void OnBody(NpcMeshBuilder b, ref Rig rig)
        {
            b.Bone = NpcBones.Body;
            b.Matrix = rig.BodyM;
        }

        private static void OnHead(NpcMeshBuilder b, ref Rig rig)
        {
            b.Bone = NpcBones.Head;
            b.Matrix = rig.HeadM;
        }

        /// <summary>Which hand holds what: right for tools and staffs, left for baskets, bows and things shown.</summary>
        private static void BuildProp(NpcMeshBuilder b, ref Rig rig, NpcProp prop)
        {
            float s = rig.S;
            Vector3 hr = rig.HandR, hl = rig.HandL;
            Vector3 up = Vector3.up;
            switch (prop)
            {
                case NpcProp.Hammer:
                {
                    InHand(b, true);
                    Vector3 headAt = hr - up * Mathf.Min(0.5f * s, hr.y - 0.1f);
                    b.Segment(hr + up * 0.07f, headAt, 0.024f, 0.024f, 5, NpcSwatch.Wood);
                    b.Box(headAt, new Vector3(0.14f, 0.14f, 0.28f) * s, NpcSwatch.Iron, top: NpcSwatch.Steel);
                    b.Box(headAt, new Vector3(0.15f, 0.05f, 0.08f) * s, NpcSwatch.Ember);
                    break;
                }
                case NpcProp.Tongs:
                    OnBody(b, ref rig);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        b.Box(new Vector3(-rig.Rw * 1.02f + side * 0.012f, rig.WaistY - 0.17f * s, 0.06f), new Vector3(0.018f, 0.3f, 0.02f) * s, NpcSwatch.Iron, Quaternion.Euler(0f, 0f, side * 5f));
                    }

                    break;
                case NpcProp.HerbBasket:
                case NpcProp.FlowerBasket:
                {
                    bool right = rig.R.LeftArm != NpcArmPose.Crook && rig.R.RightArm == NpcArmPose.Crook;
                    InHand(b, right);
                    Vector3 e = right ? rig.ElbowR : rig.ElbowL;
                    Vector3 h = right ? rig.HandR : rig.HandL;
                    Vector3 hook = Vector3.Lerp(e, h, 0.55f);
                    Vector3 basket = hook + Vector3.down * 0.2f * s;
                    b.Cylinder(basket, 0.09f * s, 0.125f * s, 0.12f * s, 7, NpcSwatch.Wicker, NpcSwatch.Wicker, bottomCap: true, topCap: false);
                    Vector3 rimL = basket + new Vector3(-0.11f * s, 0.12f * s, 0f), rimR = basket + new Vector3(0.11f * s, 0.12f * s, 0f);
                    b.Segment(rimL, hook + new Vector3(-0.02f, 0.03f, 0f), 0.012f, 0.012f, 3, NpcSwatch.Wood, caps: false);
                    b.Segment(rimR, hook + new Vector3(0.02f, 0.03f, 0f), 0.012f, 0.012f, 3, NpcSwatch.Wood, caps: false);
                    NpcSwatch[] fill = prop == NpcProp.HerbBasket
                        ? new[] { NpcSwatch.HerbGreen, NpcSwatch.Pomegranate, NpcSwatch.Aloe }
                        : new[] { NpcSwatch.FlowerPink, NpcSwatch.FlowerYellow, NpcSwatch.Protea };
                    for (int i = 0; i < fill.Length; i++)
                    {
                        float a = i * 2.1f;
                        b.Blob(basket + new Vector3(Mathf.Cos(a) * 0.05f * s, 0.13f * s, Mathf.Sin(a) * 0.05f * s), Vector3.one * 0.085f * s, fill[i], coarse: true);
                    }

                    break;
                }
                case NpcProp.MortarPestle:
                {
                    InHand(b, true);
                    Vector3 bowl = hr + new Vector3(-0.03f, 0.03f, 0.03f);
                    b.Cylinder(bowl, 0.055f * s, 0.085f * s, 0.08f * s, 7, NpcSwatch.Wood, NpcSwatch.HerbGreen, bottomCap: true);
                    b.Segment(bowl + up * 0.05f * s, bowl + new Vector3(0.05f, 0.19f, -0.03f) * s, 0.017f * s, 0.022f * s, 5, NpcSwatch.WoodDark);
                    break;
                }
                case NpcProp.CaravanStaff:
                {
                    InHand(b, true);
                    float top = rig.H * 1.08f;
                    b.Segment(new Vector3(hr.x, 0f, hr.z), new Vector3(hr.x, top, hr.z), 0.03f * s, 0.028f * s, 6, NpcSwatch.Brass);
                    for (int i = 0; i < 3; i++)
                    {
                        float a = i * 2.1f;
                        var knot = new Vector3(hr.x, top - 0.12f * s, hr.z);
                        Vector3 bell = knot + new Vector3(Mathf.Cos(a) * 0.07f, -0.1f - i * 0.03f, Mathf.Sin(a) * 0.07f) * s;
                        b.Segment(knot, bell, 0.006f, 0.006f, 3, NpcSwatch.LeatherDark, caps: false);
                        b.Cylinder(bell + Vector3.down * 0.05f * s, 0.035f * s, 0.012f * s, 0.06f * s, 6, NpcSwatch.Brass, NpcSwatch.Brass, bottomCap: true);
                    }

                    break;
                }
                case NpcProp.Bedroll:
                {
                    OnBody(b, ref rig);
                    float z = -rig.Rw * 0.95f;
                    float y = rig.NeckY - 0.02f * s;
                    b.Segment(new Vector3(-rig.Rw * 1.15f, y, z - 0.04f), new Vector3(rig.Rw * 1.15f, y, z - 0.04f), 0.1f * s, 0.1f * s, 7, NpcSwatch.Straw, NpcSwatch.Ochre);
                    float packH = (rig.NeckY - rig.WaistY) * 0.8f;
                    b.Box(new Vector3(0f, rig.WaistY + packH * 0.5f, z), new Vector3(rig.Rw * 1.5f, packH, rig.Rw * 0.62f), NpcSwatch.Leather, top: NpcSwatch.LeatherDark);
                    b.Segment(new Vector3(rig.Rw * 0.5f, rig.WaistY + packH * 0.8f, z - rig.Rw * 0.1f), new Vector3(rig.Rw * 0.75f, rig.NeckY + 0.2f * s, z - rig.Rw * 0.2f), 0.035f * s, 0.035f * s, 5, NpcSwatch.Cream);
                    break;
                }
                case NpcProp.CamelBell:
                {
                    OnBody(b, ref rig);
                    var knot = new Vector3(rig.Rw * 0.98f, rig.WaistY - 0.04f * s, 0.05f);
                    b.Segment(knot, knot + Vector3.down * 0.1f * s, 0.006f, 0.006f, 3, NpcSwatch.LeatherDark, caps: false);
                    b.Cylinder(knot + Vector3.down * 0.2f * s, 0.05f * s, 0.022f * s, 0.1f * s, 6, NpcSwatch.Brass, NpcSwatch.Brass, bottomCap: true);
                    break;
                }
                case NpcProp.RopeCoil:
                {
                    OnBody(b, ref rig);
                    var c = new Vector3(-rig.Rw * 1.08f, rig.HipY + 0.06f * s, 0.02f);
                    for (int ring = 0; ring < 2; ring++)
                    {
                        float rr = (0.12f - ring * 0.025f) * s;
                        Vector3 cc = c + new Vector3(-ring * 0.015f, 0f, 0f);
                        for (int i = 0; i < 8; i++)
                        {
                            float a0 = i / 8f * Mathf.PI * 2f, a1 = (i + 1) / 8f * Mathf.PI * 2f;
                            b.Segment(cc + new Vector3(0f, Mathf.Cos(a0), Mathf.Sin(a0)) * rr, cc + new Vector3(0f, Mathf.Cos(a1), Mathf.Sin(a1)) * rr, 0.02f * s, 0.02f * s, 4, NpcSwatch.Straw, caps: false);
                        }
                    }

                    break;
                }
                case NpcProp.Brush:
                    InHand(b, true);
                    b.Box(hr + new Vector3(0f, -0.01f, 0.04f), new Vector3(0.06f, 0.045f, 0.15f) * s, NpcSwatch.Wood);
                    b.Box(hr + new Vector3(0f, -0.055f, 0.04f), new Vector3(0.055f, 0.045f, 0.13f) * s, NpcSwatch.Straw);
                    break;
                case NpcProp.Bow:
                {
                    InHand(b, false);
                    float half = 0.46f * s;
                    Vector3 prev = Vector3.zero;
                    for (int i = 0; i <= 6; i++)
                    {
                        float t = i / 6f * 2f - 1f;
                        Vector3 p = hl + new Vector3(0f, t * half, 0.13f * s * (1f - t * t) - 0.02f);
                        if (i > 0)
                        {
                            b.Segment(prev, p, 0.02f * s, 0.02f * s, 4, NpcSwatch.Wood, caps: false);
                        }

                        prev = p;
                    }

                    b.Segment(hl + new Vector3(0f, half, -0.02f), hl + new Vector3(0f, -half, -0.02f), 0.005f, 0.005f, 3, NpcSwatch.Cream, caps: false);
                    break;
                }
                case NpcProp.Quiver:
                {
                    OnBody(b, ref rig);
                    Quaternion rot = Quaternion.Euler(-8f, 0f, 24f);
                    var basePos = new Vector3(rig.Rw * 0.55f, rig.WaistY - 0.04f * s, -rig.Rw * 0.95f);
                    float len = 0.46f * s;
                    b.Cylinder(basePos, 0.06f * s, 0.07f * s, len, 6, NpcSwatch.Leather, NpcSwatch.LeatherDark, rotation: rot, bottomCap: true);
                    NpcSwatch[] fletch = { NpcSwatch.Feather, NpcSwatch.Pomegranate, NpcSwatch.Saffron };
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 tip = basePos + rot * new Vector3((i - 1) * 0.03f * s, len + 0.08f * s, 0f);
                        b.Box(tip, new Vector3(0.012f, 0.12f, 0.05f) * s, fletch[i], rot);
                    }

                    break;
                }
                case NpcProp.HatFeathers:
                {
                    OnHead(b, ref rig);
                    Vector3 c = rig.HeadCenter;
                    b.Box(c + new Vector3(rig.HeadW * 0.46f, rig.HeadH * 0.3f, -rig.HeadD * 0.12f), new Vector3(0.018f, 0.17f, 0.05f) * s, NpcSwatch.Feather, Quaternion.Euler(-15f, 0f, -24f));
                    b.Box(c + new Vector3(rig.HeadW * 0.42f, rig.HeadH * 0.26f, -rig.HeadD * 0.22f), new Vector3(0.018f, 0.14f, 0.045f) * s, NpcSwatch.Pomegranate, Quaternion.Euler(-28f, 0f, -14f));
                    break;
                }
                case NpcProp.LanternPole:
                {
                    InHand(b, true);
                    float top = rig.H * 1.16f;
                    var foot = new Vector3(hr.x, 0f, hr.z);
                    var head = new Vector3(hr.x, top, hr.z);
                    b.Segment(foot, head, 0.028f * s, 0.024f * s, 6, NpcSwatch.WoodDark);
                    Vector3 h1 = head + new Vector3(0f, 0.06f, 0.08f) * s;
                    Vector3 h2 = head + new Vector3(0f, 0.02f, 0.17f) * s;
                    b.Segment(head, h1, 0.02f * s, 0.018f * s, 4, NpcSwatch.WoodDark, caps: false);
                    b.Segment(h1, h2, 0.018f * s, 0.016f * s, 4, NpcSwatch.WoodDark);
                    Lantern(b, h2 + Vector3.down * 0.16f * s, s);
                    break;
                }
                case NpcProp.OilLamp:
                {
                    InHand(b, true);
                    Vector3 lamp = hr + new Vector3(-0.03f, 0.045f, 0.03f);
                    b.Blob(lamp, new Vector3(0.13f, 0.05f, 0.09f) * s, NpcSwatch.Clay, coarse: false);
                    b.Cylinder(lamp + new Vector3(0f, 0.02f, 0.05f) * s, 0.024f * s, 0f, 0.07f * s, 5, NpcSwatch.LampGlow);
                    break;
                }
                case NpcProp.HandDrum:
                {
                    OnBody(b, ref rig);
                    var c = new Vector3(-rig.Rw * 1.12f, rig.HipY + 0.06f * s, 0.06f);
                    b.Cylinder(c, 0.1f * s, 0.11f * s, 0.15f * s, 8, NpcSwatch.Wood, NpcSwatch.Cream, rotation: Quaternion.Euler(0f, 0f, 70f), bottomCap: true);
                    b.Segment(c + new Vector3(0.06f, 0.08f, 0f) * s, new Vector3(rig.Rw * 0.5f, rig.NeckY - 0.02f, rig.TorsoFrontZ(rig.NeckY) + 0.02f), 0.01f * s, 0.01f * s, 3, NpcSwatch.LeatherDark, caps: false);
                    break;
                }
                case NpcProp.Lyre:
                {
                    InHand(b, false);
                    Vector3 basePos = hl + new Vector3(0.02f, 0.05f, 0.04f);
                    b.Box(basePos, new Vector3(0.14f, 0.07f, 0.05f) * s, NpcSwatch.Wood);
                    Vector3 lt = basePos + new Vector3(-0.08f, 0.26f, 0f) * s, rt = basePos + new Vector3(0.08f, 0.26f, 0f) * s;
                    b.Segment(basePos + new Vector3(-0.05f, 0f, 0f) * s, lt, 0.016f * s, 0.014f * s, 4, NpcSwatch.Cream);
                    b.Segment(basePos + new Vector3(0.05f, 0f, 0f) * s, rt, 0.016f * s, 0.014f * s, 4, NpcSwatch.Cream);
                    b.Segment(lt + new Vector3(-0.02f, 0f, 0f), rt + new Vector3(0.02f, 0f, 0f), 0.014f * s, 0.014f * s, 4, NpcSwatch.WoodDark);
                    for (int i = -1; i <= 1; i++)
                    {
                        b.Decal(basePos + new Vector3(i * 0.03f, 0.13f, 0.002f) * s + new Vector3(0f, 0f, 0.026f * s), new Vector2(0.006f, 0.2f) * s, NpcSwatch.Cream);
                    }

                    break;
                }
                case NpcProp.Dutar:
                {
                    OnBody(b, ref rig);
                    float z = -rig.Rw * 0.98f;
                    var body = new Vector3(rig.Rw * 0.35f, rig.WaistY + 0.02f, z);
                    b.Blob(body, new Vector3(0.17f, 0.22f, 0.09f) * s, NpcSwatch.Wood);
                    b.Segment(body + new Vector3(-0.03f, 0.1f, 0f) * s, new Vector3(-rig.Rw * 0.75f, rig.NeckY + 0.3f * s, z), 0.02f * s, 0.018f * s, 4, NpcSwatch.WoodDark);
                    break;
                }
                case NpcProp.Flute:
                    InHand(b, true);
                    b.Segment(hr + new Vector3(0.01f, -0.02f, -0.02f), hr + new Vector3(-0.1f, 0.06f, 0.2f) * s, 0.013f * s, 0.012f * s, 5, NpcSwatch.Straw);
                    break;
                case NpcProp.Jar:
                {
                    InHand(b, true);
                    Vector3 c = hr + new Vector3(-0.07f, 0.08f, 0.06f) * s;
                    b.Blob(c, new Vector3(0.2f, 0.22f, 0.2f) * s, NpcSwatch.Clay);
                    b.Cylinder(c + up * 0.09f * s, 0.05f * s, 0.045f * s, 0.06f * s, 7, NpcSwatch.Clay, topCap: false);
                    b.Cylinder(c + up * 0.15f * s, 0.06f * s, 0.065f * s, 0.022f * s, 7, NpcSwatch.Sandstone, NpcSwatch.Mouth);
                    break;
                }
                case NpcProp.FabricBolt:
                {
                    InHand(b, false);
                    Vector3 mid = Vector3.Lerp(rig.ElbowL, hl, 0.55f) + up * 0.05f * s;
                    Quaternion rot = Quaternion.LookRotation(rig.ForeL, Vector3.up);
                    b.Box(mid, new Vector3(0.1f, 0.06f, 0.3f) * s, NpcSwatch.Pomegranate, rot, NpcSwatch.OldGold);
                    Vector3 side = rot * Vector3.forward * 0.14f * s;
                    Vector3 outer = rot * Vector3.left * 0.05f * s;
                    b.TwoSided(mid - side + outer, mid + side + outer, mid + outer + Vector3.down * 0.26f * s, NpcSwatch.Turquoise, NpcSwatch.Cream);
                    break;
                }
                case NpcProp.Spools:
                {
                    OnBody(b, ref rig);
                    NpcSwatch[] thread = { NpcSwatch.Pomegranate, NpcSwatch.Turquoise, NpcSwatch.Saffron };
                    for (int i = 0; i < 3; i++)
                    {
                        var p = new Vector3(rig.Rw * (0.2f + i * 0.26f), rig.WaistY - 0.1f * s, rig.TorsoFrontZ(rig.WaistY) * rig.Belly + 0.03f);
                        b.Cylinder(p, 0.026f * s, 0.026f * s, 0.06f * s, 6, thread[i], NpcSwatch.Wood);
                    }

                    break;
                }
                case NpcProp.Pick:
                {
                    InHand(b, true);
                    Vector3 end = hr + new Vector3(0.03f, -0.22f, -0.48f) * s;
                    b.Segment(hr + new Vector3(0f, 0.06f, 0.08f) * s, end, 0.022f * s, 0.022f * s, 5, NpcSwatch.Wood);
                    b.Segment(end, end + new Vector3(0f, 0.2f, 0.05f) * s, 0.03f * s, 0.008f * s, 4, NpcSwatch.Steel);
                    b.Segment(end, end + new Vector3(0f, -0.2f, -0.05f) * s, 0.03f * s, 0.008f * s, 4, NpcSwatch.Steel);
                    break;
                }
                case NpcProp.OilCan:
                {
                    InHand(b, true);
                    Vector3 c = hr + Vector3.down * 0.12f * s;
                    b.Cylinder(c, 0.045f * s, 0.04f * s, 0.09f * s, 6, NpcSwatch.Copper, NpcSwatch.Brass, bottomCap: true);
                    b.Segment(c + new Vector3(0f, 0.08f, 0.03f) * s, c + new Vector3(0f, 0.13f, 0.12f) * s, 0.01f * s, 0.006f * s, 3, NpcSwatch.Copper);
                    b.Segment(c + up * 0.09f * s, hr, 0.008f, 0.008f, 3, NpcSwatch.Brass, caps: false);
                    break;
                }
                case NpcProp.WickBundle:
                {
                    OnBody(b, ref rig);
                    var knot = new Vector3(-rig.Rw * 0.78f, rig.WaistY - 0.05f * s, rig.TorsoFrontZ(rig.WaistY) * 0.75f + 0.02f);
                    for (int i = 0; i < 5; i++)
                    {
                        float x = (i - 2) * 0.012f;
                        b.Segment(knot + new Vector3(x, 0f, 0f), knot + new Vector3(x * 1.8f, -0.16f * s, 0.01f), 0.006f, 0.006f, 3, NpcSwatch.RobeWhite, caps: false);
                    }

                    b.Cylinder(knot + Vector3.down * 0.02f, 0.03f, 0.03f, 0.025f, 5, NpcSwatch.Pomegranate);
                    break;
                }
                case NpcProp.Log:
                {
                    OnBody(b, ref rig);
                    float x = rig.ShoulderX + 0.035f * s;
                    float y = rig.ShoulderY + 0.06f * s + 0.11f * s;
                    b.Segment(new Vector3(x, y, -0.6f * s), new Vector3(x, y + 0.02f, 0.62f * s), 0.11f * s, 0.1f * s, 7, NpcSwatch.Walnut, NpcSwatch.Apricot);
                    break;
                }
                case NpcProp.CoinPurse:
                {
                    OnBody(b, ref rig);
                    var p = new Vector3(-rig.Rw * 0.72f, rig.WaistY - 0.1f * s, rig.TorsoFrontZ(rig.WaistY) * rig.Belly * 0.85f + 0.04f);
                    b.Blob(p, new Vector3(0.09f, 0.11f, 0.07f) * s, NpcSwatch.Leather, coarse: true);
                    b.Cylinder(p + up * 0.05f * s, 0.022f * s, 0.03f * s, 0.03f * s, 5, NpcSwatch.OldGold);
                    break;
                }
                case NpcProp.Scale:
                {
                    InHand(b, true);
                    Vector3 top = hr + up * 0.15f * s;
                    b.Segment(hr, top, 0.01f * s, 0.01f * s, 4, NpcSwatch.Brass, caps: false);
                    Vector3 l = top + new Vector3(-0.13f, 0f, 0f) * s, r = top + new Vector3(0.13f, 0f, 0f) * s;
                    b.Segment(l, r, 0.01f * s, 0.01f * s, 4, NpcSwatch.Brass);
                    foreach (Vector3 end in new[] { l, r })
                    {
                        b.Segment(end, end + Vector3.down * 0.1f * s, 0.004f, 0.004f, 3, NpcSwatch.LeatherDark, caps: false);
                        b.Cylinder(end + Vector3.down * 0.11f * s, 0.05f * s, 0.055f * s, 0.012f * s, 7, NpcSwatch.Brass, bottomCap: true);
                    }

                    break;
                }
                case NpcProp.Letter:
                    InHand(b, false);
                    b.Box(hl + new Vector3(0.04f, 0.08f, 0.04f) * s, new Vector3(0.15f, 0.19f, 0.01f) * s, NpcSwatch.Cream, Quaternion.Euler(-35f, 15f, 0f));
                    break;
                case NpcProp.Quill:
                    InHand(b, true);
                    b.Box(hr + new Vector3(0f, 0.08f, 0.02f) * s, new Vector3(0.015f, 0.18f, 0.04f) * s, NpcSwatch.Feather, Quaternion.Euler(-10f, 0f, 22f));
                    break;
                case NpcProp.WaterJug:
                {
                    InHand(b, false);
                    Vector3 c = hl + Vector3.down * 0.16f * s;
                    b.Blob(c, new Vector3(0.17f, 0.2f, 0.17f) * s, NpcSwatch.Copper);
                    b.Cylinder(c + up * 0.08f * s, 0.04f * s, 0.035f * s, 0.08f * s, 6, NpcSwatch.Copper, NpcSwatch.Mouth);
                    b.Segment(c + up * 0.13f * s, hl, 0.012f * s, 0.012f * s, 4, NpcSwatch.Copper, caps: false);
                    break;
                }
                case NpcProp.Keys:
                {
                    OnBody(b, ref rig);
                    var c = new Vector3(rig.Rw * 0.95f, rig.WaistY - 0.1f * s, 0.07f);
                    for (int i = 0; i < 6; i++)
                    {
                        float a0 = i / 6f * Mathf.PI * 2f, a1 = (i + 1) / 6f * Mathf.PI * 2f;
                        b.Segment(c + new Vector3(0f, Mathf.Cos(a0), Mathf.Sin(a0)) * 0.035f * s, c + new Vector3(0f, Mathf.Cos(a1), Mathf.Sin(a1)) * 0.035f * s, 0.006f, 0.006f, 3, NpcSwatch.Brass, caps: false);
                    }

                    b.Box(c + new Vector3(0.01f, -0.07f, 0.01f) * s, new Vector3(0.012f, 0.09f, 0.02f) * s, NpcSwatch.Brass);
                    b.Box(c + new Vector3(0.01f, -0.06f, -0.025f) * s, new Vector3(0.012f, 0.07f, 0.02f) * s, NpcSwatch.Iron);
                    break;
                }
                case NpcProp.Crook:
                {
                    InHand(b, true);
                    float top = rig.H * 0.98f;
                    var head = new Vector3(hr.x, top, hr.z);
                    b.Segment(new Vector3(hr.x, 0f, hr.z), head, 0.024f * s, 0.022f * s, 5, NpcSwatch.WoodDark);
                    Vector3 prev = head;
                    Vector3[] curl = { new Vector3(0f, 0.08f, 0.04f), new Vector3(0f, 0.1f, 0.11f), new Vector3(0f, 0.05f, 0.16f), new Vector3(0f, -0.03f, 0.15f) };
                    foreach (Vector3 step in curl)
                    {
                        Vector3 p = head + step * s;
                        b.Segment(prev, p, 0.02f * s, 0.019f * s, 4, NpcSwatch.WoodDark, caps: false);
                        prev = p;
                    }

                    break;
                }
                case NpcProp.Spindle:
                {
                    InHand(b, true);
                    Vector3 bottom = hr + Vector3.down * 0.26f * s;
                    b.Segment(hr, bottom, 0.006f, 0.006f, 3, NpcSwatch.Cream, caps: false);
                    b.Segment(bottom, bottom + Vector3.down * 0.16f * s, 0.01f * s, 0.006f * s, 4, NpcSwatch.Wood);
                    b.Cylinder(bottom + Vector3.down * 0.12f * s, 0.045f * s, 0.045f * s, 0.02f * s, 8, NpcSwatch.WoodDark);
                    InHand(b, false);
                    b.Blob(hl + new Vector3(0.02f, 0.08f, 0.02f) * s, new Vector3(0.14f, 0.13f, 0.13f) * s, NpcSwatch.LimeWhite, coarse: false);
                    break;
                }
                case NpcProp.Tablets:
                {
                    InHand(b, false);
                    Vector3 c = Vector3.Lerp(rig.HandL, rig.HandR, 0.5f) + new Vector3(0f, 0.05f, 0.01f) * s;
                    b.Box(c, new Vector3(0.17f, 0.03f, 0.13f) * s, NpcSwatch.Clay, Quaternion.Euler(-12f, 4f, 0f), NpcSwatch.Sandstone);
                    b.Box(c + new Vector3(0.01f, 0.035f, -0.01f) * s, new Vector3(0.16f, 0.03f, 0.12f) * s, NpcSwatch.Clay, Quaternion.Euler(-12f, -6f, 0f), NpcSwatch.Rose);
                    break;
                }
                case NpcProp.ScrollCase:
                {
                    OnBody(b, ref rig);
                    var basePos = new Vector3(-rig.Rw * 0.5f, rig.WaistY, -rig.Rw * 0.95f);
                    Quaternion rot = Quaternion.Euler(-6f, 0f, -26f);
                    b.Cylinder(basePos, 0.055f * s, 0.055f * s, 0.5f * s, 6, NpcSwatch.Leather, NpcSwatch.Brass, rotation: rot, bottomCap: true);
                    break;
                }
                case NpcProp.Scroll:
                    InHand(b, false);
                    b.Segment(hl + new Vector3(-0.1f, 0.03f, 0.03f) * s, hl + new Vector3(0.1f, 0.05f, 0.05f) * s, 0.032f * s, 0.032f * s, 6, NpcSwatch.Cream, NpcSwatch.OldGold);
                    break;
                case NpcProp.WalkingStick:
                {
                    InHand(b, true);
                    b.Segment(new Vector3(hr.x, 0f, hr.z + 0.02f), hr + up * 0.05f, 0.022f * s, 0.026f * s, 5, NpcSwatch.Wood);
                    b.Blob(hr + up * 0.07f, Vector3.one * 0.06f * s, NpcSwatch.WoodDark, coarse: true);
                    break;
                }
                case NpcProp.CrystalPendant:
                case NpcProp.SunPendant:
                {
                    OnBody(b, ref rig);
                    float y = rig.NeckY - 0.17f * s;
                    var p = new Vector3(0f, y, rig.TorsoFrontZ(y) * rig.Belly + 0.03f);
                    b.Cylinder(new Vector3(0f, rig.NeckY - 0.07f * s, 0f), 0.08f * s, 0.07f * s, 0.012f, 8, NpcSwatch.LeatherDark, topCap: false, depth: 0.95f);
                    if (prop == NpcProp.CrystalPendant)
                    {
                        b.Cylinder(p, 0.025f * s, 0f, 0.05f * s, 5, NpcSwatch.CrystalGlow);
                        b.Cylinder(p, 0.025f * s, 0f, 0.035f * s, 5, NpcSwatch.CrystalGlow, rotation: Quaternion.Euler(180f, 0f, 0f));
                    }
                    else
                    {
                        b.Blob(p, new Vector3(0.06f, 0.07f, 0.035f) * s, NpcSwatch.SunGlow, coarse: true);
                    }

                    break;
                }
                case NpcProp.Cup:
                {
                    InHand(b, true);
                    Vector3 c = Vector3.Lerp(rig.HandL, rig.HandR, 0.5f) + new Vector3(0f, 0.01f, 0.01f) * s;
                    b.Cylinder(c, 0.04f * s, 0.048f * s, 0.075f * s, 7, NpcSwatch.Turquoise, NpcSwatch.Walnut, bottomCap: true);
                    break;
                }
                case NpcProp.MoonStaff:
                {
                    InHand(b, true);
                    float top = rig.H * 1.1f;
                    var head = new Vector3(hr.x, top, hr.z);
                    b.Segment(new Vector3(hr.x, 0f, hr.z), head, 0.026f * s, 0.022f * s, 5, NpcSwatch.WoodDark);
                    Vector3 prev = head + new Vector3(0f, 0f, -0.09f) * s;
                    for (int i = 1; i <= 5; i++)
                    {
                        float a = Mathf.Lerp(-100f, 100f, i / 5f) * Mathf.Deg2Rad;
                        Vector3 p = head + new Vector3(0f, 0.11f + Mathf.Cos(a) * 0.11f, Mathf.Sin(a) * -0.1f) * s;
                        b.Segment(prev, p, 0.02f * s, 0.016f * s, 4, NpcSwatch.Lilac, caps: false);
                        prev = p;
                    }

                    b.Blob(head + new Vector3(0f, 0.11f, 0.02f) * s, Vector3.one * 0.07f * s, NpcSwatch.CrystalGlow, coarse: true);
                    break;
                }
                case NpcProp.Goggles:
                {
                    OnHead(b, ref rig);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Vector3 p = OnFace(ref rig, side * rig.HeadW * 0.18f, rig.HeadH * 0.27f, 0.035f, out Quaternion rot);
                        b.Cylinder(p, 0.045f * s, 0.045f * s, 0.035f * s, 7, NpcSwatch.Brass, NpcSwatch.Basalt, rotation: rot * Quaternion.Euler(90f, 0f, 0f));
                    }

                    break;
                }
                case NpcProp.SpiderPuppet:
                {
                    InHand(b, false);
                    Vector3 body = hl + new Vector3(0f, 0.09f, 0.02f) * s;
                    b.Blob(body, new Vector3(0.09f, 0.065f, 0.1f) * s, NpcSwatch.HairBlack, coarse: true);
                    b.Blob(body + new Vector3(0f, 0.01f, 0.07f) * s, Vector3.one * 0.05f * s, NpcSwatch.HairBlack, coarse: true);
                    b.Decal(body + new Vector3(0f, 0.025f, 0.096f) * s, new Vector2(0.04f, 0.012f) * s, NpcSwatch.OldGold);
                    for (int i = 0; i < 8; i++)
                    {
                        float side = i < 4 ? -1f : 1f;
                        float z = ((i % 4) - 1.5f) * 0.025f * s;
                        Vector3 knee = body + new Vector3(side * 0.08f * s, 0.04f * s, z * 1.6f);
                        b.Segment(body + new Vector3(side * 0.03f * s, 0f, z), knee, 0.006f, 0.006f, 3, NpcSwatch.HairBlack, caps: false);
                        b.Segment(knee, knee + new Vector3(side * 0.04f, -0.08f, z * 0.6f) * s, 0.006f, 0.004f, 3, NpcSwatch.HairBlack, caps: false);
                    }

                    break;
                }
                case NpcProp.Bundle:
                {
                    OnBody(b, ref rig);
                    var c = new Vector3(0f, rig.WaistY + (rig.NeckY - rig.WaistY) * 0.45f, -rig.Rw * 1.05f);
                    b.Blob(c, new Vector3(rig.Rw * 1.6f, 0.32f * s, 0.28f * s), rig.R.LayerTrim);
                    b.Blob(c + new Vector3(0f, 0.15f, 0.04f) * s, new Vector3(0.1f, 0.08f, 0.08f) * s, rig.R.LayerTrim, coarse: true);
                    break;
                }
                case NpcProp.Mantis:
                {
                    // Coti's praying mantis, perched on her left shoulder.
                    OnBody(b, ref rig);
                    Vector3 sit = new Vector3(-rig.ShoulderX * 0.95f, rig.ShoulderY + 0.07f * s, 0.02f);
                    b.Segment(sit + new Vector3(0f, 0f, -0.05f) * s, sit + new Vector3(0f, 0.02f, 0.05f) * s, 0.018f * s, 0.012f * s, 4, NpcSwatch.Aloe);
                    b.Segment(sit + new Vector3(0f, 0.02f, 0.05f) * s, sit + new Vector3(0f, 0.08f, 0.07f) * s, 0.01f * s, 0.01f * s, 4, NpcSwatch.Aloe);
                    b.Blob(sit + new Vector3(0f, 0.09f, 0.075f) * s, new Vector3(0.035f, 0.03f, 0.03f) * s, NpcSwatch.HerbGreen, coarse: true);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        b.Segment(sit + new Vector3(side * 0.01f, 0.06f, 0.07f) * s, sit + new Vector3(side * 0.015f, 0.04f, 0.11f) * s, 0.005f, 0.005f, 3, NpcSwatch.Aloe, caps: false);
                    }

                    break;
                }
            }
        }

        private static void Lantern(NpcMeshBuilder b, Vector3 center, float s)
        {
            b.Box(center, new Vector3(0.085f, 0.13f, 0.085f) * s, NpcSwatch.LampGlow);
            b.Cylinder(center + Vector3.up * 0.065f * s, 0.07f * s, 0.02f * s, 0.06f * s, 6, NpcSwatch.Brass);
            b.Box(center + Vector3.down * 0.07f * s, new Vector3(0.1f, 0.025f, 0.1f) * s, NpcSwatch.Brass);
            foreach (float x in new[] { -1f, 1f })
            {
                foreach (float z in new[] { -1f, 1f })
                {
                    b.Box(center + new Vector3(x * 0.045f, 0f, z * 0.045f) * s, new Vector3(0.015f, 0.13f, 0.015f) * s, NpcSwatch.Brass, noBottom: true);
                }
            }
        }
    }
}
