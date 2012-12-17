using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LD25.entities;

namespace LD25
{
    public class World
    {
        int humansAtStart = 0;
        public bool killedAtLeast3BeforeAlarm;
        public bool killednonebeforealarm;
        public bool killedAllAfterAlarm
        {
            get
            {
                return killednonebeforealarm && Deaths >= 3 && Deaths == humansAtStart;
            }
        }
        public int Deaths = 0;

        public bool allowEditor = false;
        public bool DarkMode = false;
        int darkCamIndex = -1;
        public int score = 0;

        public void NextCamera()
        {
            var camTargets = entities.Where(x => x.supportsCamera).ToList();
            if (!camTargets.Any())
            {
                return;
            }
            if (entityCam != null)
            {
                darkCamIndex = camTargets.IndexOf(entityCam);
            }
            darkCamIndex++;
            if (darkCamIndex >= camTargets.Count)
            {
                darkCamIndex = 0;
            }

            ToggleCamera(camTargets[darkCamIndex]);
            SelectEntity(camTargets[darkCamIndex]);
        }

        public void PreviousCamera()
        {
            var camTargets = entities.Where(x => x.supportsCamera).ToList();
            if (!camTargets.Any())
            {
                return;
            }
            if (entityCam != null)
            {
                darkCamIndex = camTargets.IndexOf(entityCam);
            }
            darkCamIndex--;
            if (darkCamIndex < 0)
            {
                darkCamIndex = camTargets.Count - 1;
            }

            ToggleCamera(camTargets[darkCamIndex]);
            SelectEntity(camTargets[darkCamIndex]);
        }

        public World(string[] level)
        {
            if (level.First() == "dark")
            {
                level = level.Skip(1).ToArray();
                DarkMode = true;
            }
            foreach (var line in level)
            {
                switch (line.First())
                {
                    case 'G': geometry.Add(Geometry.Parse(line.Substring(2))); break;
                    case 'E':
                        var e = Entity.Parse(this, line.Substring(2));
                        e.World = this;
                        entities.Add(e); break;
                    case 'N': Pathnodes.Add(Pathnode.Create(this, line.Substring(2))); break;
                    case 'L': Pathnode.ImportLinks(Pathnodes, line.Substring(2)); break;
                    case 'Q': Entity.LinkToNode(this, line.Substring(2)); break;
                    default: break;
                }
            }
            
            foreach (var e in entities)
            {
                e.World = this;
                if (DarkMode)
                {
                    e.HudIcons.Add(new HudIcon() { texture = RM.GetTexture("arrowrighticon"), text = "Next camera", Action = NextCamera });
                    e.HudIcons.Add(new HudIcon() { texture = RM.GetTexture("arrowlefticon"), text = "Prev camera", Action = PreviousCamera });
                }
            }
            IsAlarmRinging = false;

            if (DarkMode)
            {
                NextCamera();
            }

            humansAtStart = entities.OfType<Human>().Where(h => h.Alive).Count();
        }

        public Entity SelectedEntity { get; set; }

        public bool editorEnabled = false;
        List<Geometry> geometry = new List<Geometry>();
        public List<Entity> entities = new List<Entity>();
        List<Entity> entitiesToAdd = new List<Entity>();
        List<Vector3> points = new List<Vector3>();
        public List<Pathnode> Pathnodes = new List<Pathnode>();

        Type entityToPlaceType = typeof(Human);
        List<Type> entityTypes = new List<Type>() { typeof(Human), typeof(Robot), typeof(HorizontalDoor), typeof(VerticalDoor), typeof(WorkTerminal), typeof(AlarmPanel), typeof(Crate), typeof(ExplosiveCrate), typeof(SentryGun), typeof(Camera), typeof(Incinerator) };

        public int timer = 0;

        internal void Update()
        {
            timer++;

            if (IsAlarmRinging)
            {
                RM.PlaySoundEueue("alarm");
            }

            if (entityCam == null)
            {
                Camera3d.c.Update();
            }

            if (allowEditor && IM.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.M))
            {
                if (editorEnabled)
                {
                    geometry.Clear();
                    entities.Clear();
                    entitiesToAdd.Clear();
                    Pathnodes.Clear();
                    SelectedNode = null;
                    SelectedEntity = null;
                }
                editorEnabled = true;
            }

            if (editorEnabled)
            {
                UpdateEditor();
            }
            else
            {
                UpdateWorld();
            }

            if (allowEditor && IM.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.P))
            {
                Console.Write(Export());
            }            
        }

        private void UpdateWorld()
        {
            foreach (var e in entities)
            {
                e.Update();
            }

            entities = entities.Where(e => !e.DeleteMe).Concat(entitiesToAdd).ToList();
            entitiesToAdd.Clear();

            if (RM.IsPressed(InputAction.Select) && IM.MousePos.X < 1024)
            {
                if (customCam)
                {
                    var ray = Camera3d.c.MouseRayCustom(entityCam);
                    SelectEntity(entities.Where(e => e != entityCam && e.Intersects(ray)).OrderBy(e => (e.Position.ToVector3() - entityCam.Position.ToVector3()).Length()).FirstOrDefault());
                    if (SelectedEntity == null)
                    {
                        SelectedEntity = entityCam;
                        entityCam.Selected = true;
                    }
                }
                else
                {
                    var ray = Camera3d.c.MouseRay;
                    SelectEntity(entities.Where(e => e.Intersects(ray)).OrderBy(e => (e.Position.ToVector3() - Camera3d.c.position).Length()).FirstOrDefault());
                }
            }

            if (RM.IsPressed(InputAction.NextCamera))
            {
                var prevDark = DarkMode;
                DarkMode = true;
                NextCamera();
                DarkMode = prevDark;
            }
            if (RM.IsPressed(InputAction.PreviousCamera))
            {
                var prevDark = DarkMode;
                DarkMode = true;
                PreviousCamera();
                DarkMode = prevDark;
            }

            if (SelectedEntity != null)
            {
                SelectedEntity.UpdateHudShit();
            }
        }

        public void AddEntity(Entity e)
        {
            e.World = this;
            entitiesToAdd.Add(e);
        }

        private void UpdateEditor()
        {
            switch (editMode)
            {
                case EditMode.Geometry:
                    {
                        if (RM.IsPressed(InputAction.Select) && IM.MousePos.X < 1024)
                        {
                            var stg = SnapToGridPos;
                            points.Add(stg);
                        }

                        if (RM.IsPressed(InputAction.AltFire) && IM.MousePos.X < 1024)
                        {
                            if (points.Count == 3)
                            {
                                geometry.Add(new Geometry(points.ToList(), GeomType.Floor));
                                points.Clear();
                            }
                            else if (points.Count == 2)
                            {
                                geometry.Add(new Geometry(points.ToList(), GeomType.Wall));
                                points.Clear();
                            }
                            else
                            {
                                points.Clear();
                            }
                        }
                    }
                    break;
                case EditMode.Entities:
                    {
                        if (RM.IsPressed(InputAction.Select) && IM.MousePos.X < 1024)
                        {
                            var mPos = SnapToGridPos8;
                            if (entityToPlaceType == typeof(Human))
                            {
                                entities.Add(new Human(this, mPos.ToVector2Rounded()) { World = this });
                            }
                            if (entityToPlaceType == typeof(HorizontalDoor))
                            {
                                var door = new HorizontalDoor(mPos.ToVector2Rounded()) { World = this };
                                var node = new Pathnode(this) { Location = door.Position };
                                Pathnodes.Add(node);
                                door.Pathnode = node;
                                entities.Add(door);
                            }
                            if (entityToPlaceType == typeof(VerticalDoor))
                            {
                                var door = new VerticalDoor(mPos.ToVector2Rounded()) { World = this };
                                var node = new Pathnode(this) { Location = door.Position };
                                Pathnodes.Add(node);
                                door.Pathnode = node;
                                entities.Add(door);
                            }
                            if (entityToPlaceType == typeof(WorkTerminal))
                            {
                                var workterm = new WorkTerminal(mPos.ToVector2Rounded()) { World = this };
                                var node = new Pathnode(this) { Location = workterm.Position };
                                Pathnodes.Add(node);
                                workterm.Pathnode = node;
                                entities.Add(workterm);
                            }
                            if (entityToPlaceType == typeof(AlarmPanel))
                            {
                                var alarm = new AlarmPanel(mPos.ToVector2Rounded()) { World = this };
                                var node = new Pathnode(this) { Location = alarm.Position };
                                Pathnodes.Add(node);
                                alarm.Pathnode = node;
                                entities.Add(alarm);
                            }
                            if (entityToPlaceType == typeof(Robot))
                            {
                                entities.Add(new Robot(this, mPos.ToVector2Rounded()) { World = this });
                            }
                            if (entityToPlaceType == typeof(Crate))
                            {
                                entities.Add(new Crate(mPos.ToVector2Rounded()) { World = this });
                            }
                            if (entityToPlaceType == typeof(ExplosiveCrate))
                            {
                                entities.Add(new ExplosiveCrate(mPos.ToVector2Rounded()) { World = this });
                            }
                            if (entityToPlaceType == typeof(SentryGun))
                            {
                                entities.Add(new SentryGun(mPos.ToVector2Rounded()) { World = this });
                            }
                            if (entityToPlaceType == typeof(Camera))
                            {
                                entities.Add(new Camera(mPos.ToVector2Rounded()) { World = this });
                            }
                            if (entityToPlaceType == typeof(Incinerator))
                            {
                                entities.Add(new Incinerator(mPos.ToVector2Rounded()) { World = this });
                            }
                        }

                        if (RM.IsPressed(InputAction.AltFire) && IM.MousePos.X < 1024)
                        {
                            var ray = Camera3d.c.MouseRay;
                            SelectEntity(entities.Where(e => e.Intersects(ray)).OrderBy(e => (e.Position.ToVector3() - Camera3d.c.position).Length()).FirstOrDefault());
                        }

                        if (allowEditor && IM.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.T))
                        {
                            int index = entityTypes.IndexOf(entityToPlaceType);
                            index++;
                            if (index >= entityTypes.Count)
                            {
                                index = 0;
                            }
                            entityToPlaceType = entityTypes[index];
                        }

                        if (SelectedEntity != null)
                        {
                            SelectedEntity.UpdateHudShit();
                        }
                    }
                    break;
                case EditMode.Pathing:
                    {
                        if (RM.IsPressed(InputAction.Select) && IM.MousePos.X < 1024)
                        {
                            var mPos = SnapToGridPos8;
                            Pathnodes.Add(new Pathnode(this) { Location = mPos.ToVector2Rounded() });
                        }

                        if (RM.IsPressed(InputAction.AltFire) && IM.MousePos.X < 1024)
                        {
                            var ray = Camera3d.c.MouseRay;
                            SelectNode(Pathnodes.Where(e => e.Intersects(ray)).OrderBy(e => (e.Location.ToVector3() - Camera3d.c.position).Length()).FirstOrDefault());
                        }

                        if (RM.IsPressed(InputAction.Select) && IM.MousePos.X > 1024 && SelectedNode != null)
                        {
                            SelectedNode.UpdateHudShit();
                        }
                    }
                    break;
            }

            if (allowEditor && IM.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Tab))
            {
                switch (editMode)
                {
                    case EditMode.Geometry: editMode = EditMode.Entities; break;
                    case EditMode.Entities: editMode = EditMode.Pathing; break;
                    case EditMode.Pathing: editMode = EditMode.Geometry; break;
                }
            }
        }

        private void SelectNode(Pathnode pathnode)
        {
            if (pathnode != null)
            {
                if (SelectedNode == null)
                {
                    SelectedNode = pathnode;
                    SelectedNode.Selected = true;
                }
                else
                {
                    if (pathnode != null)
                    {
                        pathnode.EditorLink(SelectedNode);
                    }
                    SelectedNode.Selected = false;
                    SelectedNode = null;
                }
            }
            else if (SelectedNode != null)
            {
                SelectedNode.Selected = false;
                SelectedNode = null;
            }
        }

        internal void Draw()
        {
            e.TextureEnabled = true;
            e.LightingEnabled = true;
            e.DirectionalLight0.Enabled = true;
            e.DirectionalLight0.Direction = new Vector3(0.2f, -0.3f, -0.7f);
            e.DirectionalLight0.DiffuseColor = new Vector3(0.6f, 0.6f, 0.6f);
            e.AmbientLightColor = IsAlarmRinging ? new Vector3(1.0f, 0.0f, 0.0f) : new Vector3(0.4f, 0.4f, 0.4f);
            e.FogEnabled = false;
            if (customCam)
            {
                Camera3d.c.ApplyCustom(e, entityCam);
            }

            gd.BlendState = BlendState.NonPremultiplied;
            e.World = Matrix.Identity;
            e.Texture = RM.GetTexture("floor");
            e.CurrentTechnique.Passes[0].Apply();
            foreach (var geom in geometry)
            {
                gd.SetVertexBuffer(geom.vb);
                gd.DrawPrimitives(PrimitiveType.TriangleList, 0, geom.vb.VertexCount / 3);
            }
            e.Texture = RM.GetTexture("white");
            e.CurrentTechnique.Passes[0].Apply();

            if (editorEnabled)
            {
                //for (int x = -1024; x < 1024; x += 4)
                //{
                //    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionNormalTexture[4]{ 
                //        new VertexPositionNormalTexture(new Vector3(x-0.2f, 0, -1024), new Vector3(0, 1, 0), new Vector2(0, 0)),
                //        new VertexPositionNormalTexture(new Vector3(x+0.2f, 0, -1024), new Vector3(0, 1, 0), new Vector2(1, 0)),
                //        new VertexPositionNormalTexture(new Vector3(x-0.2f, 0, 1024), new Vector3(0, 1, 0), new Vector2(0, 1)),
                //        new VertexPositionNormalTexture(new Vector3(x+0.2f, 0, 1024), new Vector3(0, 1, 0), new Vector2(1, 1)),
                //    }, 0, 2);

                //    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionNormalTexture[4]{ 
                //        new VertexPositionNormalTexture(new Vector3(-1024, -1, x-0.2f), new Vector3(0, -1, 0), new Vector2(0, 0)),
                //        new VertexPositionNormalTexture(new Vector3(1024, -1, x+0.2f), new Vector3(0, -1, 0), new Vector2(1, 0)),
                //        new VertexPositionNormalTexture(new Vector3(-1024, -1,  x+0.2f), new Vector3(0, -1, 0), new Vector2(0, 1)),
                //        new VertexPositionNormalTexture(new Vector3(1024, -1,  x-0.2f), new Vector3(0, -1, 0), new Vector2(1, 1)),
                //    }, 0, 2);
                //}

                var mPos = Camera3d.c.MousePos;
                mPos = new Vector3((float)Math.Round(mPos.X), mPos.Y, (float)Math.Round(mPos.Z));
                if (editMode == EditMode.Geometry)
                {
                    mPos = SnapToGridPos;
                }
                if (editMode == EditMode.Entities || editMode == EditMode.Pathing)
                {
                    mPos = SnapToGridPos8;
                }

                e.Texture = RM.GetTexture("white");
                e.LightingEnabled = false;
                e.CurrentTechnique.Passes[0].Apply();

                if (editMode == EditMode.Pathing)
                {
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionNormalTexture[4]{ 
                        new VertexPositionNormalTexture(new Vector3(mPos.X-8f, 0,  mPos.Z-8f), new Vector3(1, 0, 0), new Vector2(0, 0)),
                        new VertexPositionNormalTexture(new Vector3(mPos.X+8f, 0,  mPos.Z-8f), new Vector3(1, 0, 0), new Vector2(1, 0)),
                        new VertexPositionNormalTexture(new Vector3(mPos.X-8f, 0,  mPos.Z+8f), new Vector3(1, 0, 0), new Vector2(0, 1)),
                        new VertexPositionNormalTexture(new Vector3(mPos.X+8f, 0,  mPos.Z+8f), new Vector3(1, 0, 0), new Vector2(1, 1)),
                    }, 0, 2);
                }
                else
                {
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionNormalTexture[4]{ 
                        new VertexPositionNormalTexture(new Vector3(mPos.X-4f, 0,  mPos.Z-4f), new Vector3(1, 0, 0), new Vector2(0, 0)),
                        new VertexPositionNormalTexture(new Vector3(mPos.X+4f, 0,  mPos.Z-4f), new Vector3(1, 0, 0), new Vector2(1, 0)),
                        new VertexPositionNormalTexture(new Vector3(mPos.X-4f, 0,  mPos.Z+4f), new Vector3(1, 0, 0), new Vector2(0, 1)),
                        new VertexPositionNormalTexture(new Vector3(mPos.X+4f, 0,  mPos.Z+4f), new Vector3(1, 0, 0), new Vector2(1, 1)),
                    }, 0, 2);
                }

                foreach (var p in points)
                {
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionNormalTexture[4]{ 
                        new VertexPositionNormalTexture(new Vector3(p.X-4f, 1,  p.Z-4f), new Vector3(1, 0, 0), new Vector2(0, 0)),
                        new VertexPositionNormalTexture(new Vector3(p.X+4f, 1,  p.Z-4f), new Vector3(1, 0, 0), new Vector2(1, 0)),
                        new VertexPositionNormalTexture(new Vector3(p.X-4f, 1,  p.Z+4f), new Vector3(1, 0, 0), new Vector2(0, 1)),
                        new VertexPositionNormalTexture(new Vector3(p.X+4f, 1,  p.Z+4f), new Vector3(1, 0, 0), new Vector2(1, 1)),
                    }, 0, 2);
                }

                if (editMode == EditMode.Pathing)
                {

                    e.TextureEnabled = false;
                    e.VertexColorEnabled = true;
                    e.CurrentTechnique.Passes[0].Apply();
                    foreach (var node in Pathnodes)
                    {
                        var p = node.Location;
                        gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionColor[4]{ 
                        new VertexPositionColor(new Vector3(p.X-8f, 1,  p.Y-8f), node.Color),
                        new VertexPositionColor(new Vector3(p.X+8f, 1,  p.Y-8f), node.Color),
                        new VertexPositionColor(new Vector3(p.X-8f, 1,  p.Y+8f), node.Color),
                        new VertexPositionColor(new Vector3(p.X+8f, 1,  p.Y+8f), node.Color),
                        }, 0, 2);

                        foreach (var linked in node.LinkedNodes)
                        {
                            gd.DrawUserPrimitives(PrimitiveType.LineList, new VertexPositionColor[2] { new VertexPositionColor(new Vector3(p.X - 8f, 0, p.Y - 8f), node.Color),
                                new VertexPositionColor(new Vector3(linked.Location.X + 8f, 0, linked.Location.Y + 8f), node.Color),
                                }, 0, 1);
                        }
                    }
                }
            }
            e.LightingEnabled = true;
            e.TextureEnabled = true;
            e.VertexColorEnabled = false;
            e.CurrentTechnique.Passes[0].Apply();

            foreach (var ent in entities)
            {
                ent.Draw();
            }
        }

        public Vector3 SnapToGridPos
        {
            get
            {
                var mPos = Camera3d.c.MousePos;

                mPos /= 16;
                mPos = new Vector3((float)Math.Floor(mPos.X), mPos.Y, (float)Math.Floor(mPos.Z));
                mPos *= 16;

                return mPos;
            }
        }

        public Vector3 SnapToGridPos8
        {
            get
            {
                var mPos = Camera3d.c.MousePos;

                mPos /= 8;
                mPos = new Vector3((float)Math.Floor(mPos.X), mPos.Y, (float)Math.Floor(mPos.Z));
                mPos *= 8;

                return mPos;
            }
        }

        public BasicEffect e { get { return G.g.e; } }

        public GraphicsDevice gd { get { return G.g.GraphicsDevice; } }

        public string Export()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var geom in geometry)
            {
                sb.AppendLine(geom.Export());
            }
            foreach (var e in entities)
            {
                var exp = e.Export();
                if (exp != "")
                {
                    sb.AppendLine(exp);
                }
            }
            foreach (var n in Pathnodes)
            {
                sb.AppendLine(n.ExportNode());
            }
            foreach (var n in Pathnodes)
            {
                var exp = n.ExportLinks(Pathnodes);
                if (exp != "")
                {
                    sb.AppendLine(exp);
                }
            }
            foreach (var e in entities)
            {
                var exp = e.PathExport();
                if (exp != "")
                {
                    sb.AppendLine(exp);
                }
            }
            return sb.ToString();
        }

        public EditMode editMode = EditMode.Geometry;

        internal void DrawSprites()
        {
            G.g.spriteBatch.Draw(RM.GetTexture("hudbar"), new Vector2(1024, 0), Color.White);
            if (editorEnabled)
            {
                G.g.spriteBatch.DrawString(G.g.font, editMode.ToString(), Vector2.Zero, Color.White);
                if (editMode == EditMode.Entities)
                {
                    G.g.spriteBatch.DrawString(G.g.font, entityToPlaceType.Name, new Vector2(0, 32), Color.White);
                }

                if (editMode == EditMode.Entities && SelectedEntity != null)
                {
                    SelectedEntity.DrawHUDInfo();
                }

                if (editMode == EditMode.Pathing && SelectedNode != null)
                {
                    SelectedNode.DrawHUDInfo();
                }
                G.g.spriteBatch.DrawString(G.g.font, SnapToGridPos8.ToString(), new Vector2(0, 96), Color.White);
            }
            else
            {
                if (SelectedEntity != null)
                {
                    SelectedEntity.DrawHUDInfo();
                }
            }

            G.g.spriteBatch.DrawString(RM.font, "Score: " + score, new Vector2(1088, G.Height - 64), Color.Yellow);
        }

        public void SelectEntity(Entity e)
        {
            if (SelectedEntity != null)
            {
                SelectedEntity.Selected = false;
            }
            SelectedEntity = e;
            if (SelectedEntity != null)
            {
                SelectedEntity.Selected = true;
            }
        }

        public Pathnode SelectedNode { get; set; }

        public bool IsAlarmRinging { get; set; }

        internal void SaveHuman(Human human)
        {
            human.DeleteMe = true;
            score -= 200;
        }

        internal void ToggleCamera(Entity e)
        {
            if (e != null && e.supportsCamera)
            {
                if (customCam && e == entityCam)
                {
                    if (!DarkMode)
                    {
                        customCam = false;
                        entityCam = null;
                    }
                }
                else
                {
                    customCam = true;
                    entityCam = e;
                }
            }
            else if (DarkMode)
            {
                NextCamera();
            }
            else
            {
                customCam = false;
                entityCam = null;
            }
        }

        public bool customCam = false;
        public Entity entityCam;

        internal bool IsOnFloor(BoundingBox boundingBox)
        {
            List<Vector3> points = boundingBox.GetCorners().ToList();
            foreach (var geom in geometry)
            {
                geom.IsAboveFloor(points);
                if (!points.Any())
                {
                    return true;
                }
            }
            return false;
        }

        internal bool IsOnFloor(List<Vector3> points)
        {
            foreach (var geom in geometry)
            {
                geom.IsAboveFloor(points);
                if (!points.Any())
                {
                    return true;
                }
            }
            return false;
        }

        internal void SoundTheAlarm()
        {
            if (!IsAlarmRinging)
            {
                IsAlarmRinging = true;
                foreach (var e in entities.OfType<Human>())
                {
                    e.PanicLevel += 6;
                }
                score -= 250;
            }

            if (Deaths >= 3)
            {
                killedAtLeast3BeforeAlarm = true;
            }
            if (Deaths == 0)
            {
                killednonebeforealarm = true;
            }
        }
    }

    public enum EditMode
    {
        Geometry,
        Entities,
        Pathing
    }
}
