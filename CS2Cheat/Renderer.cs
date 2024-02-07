using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace CS2Cheat
{
    // ImGui绘制透视框
    public class Renderer : Overlay
    {
        // 瞄准敌人
        public bool aimbot = true;
        // 瞄准队友
        public bool aimOnTeam = false;
        // 开启方框透视
        private bool enableBoxPerspective = true;
        // 敌人的方框颜色 rgba 默认为红色
        private Vector4 enemyBoxColor = new Vector4(1, 0, 0, 1);
        // 好兄弟们的方框颜色 rgba 默认为蓝色
        private Vector4 brotherBoxColor = new Vector4(0, 0, 1, 1);  
        // 屏幕尺寸 注意，我的电脑是125%的缩放才到1920*1080
        public Vector2 screenSize = new Vector2(1536, 864); 
        // entity线程安全的copy
        private Entity localPlayer = new Entity();
        private ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        private readonly object entityLock = new object();
        // imgui组件
        ImDrawListPtr drawList;

        protected override void Render()
        {
            ImGui.Begin("CS2 Cheat");

            ImGui.Checkbox("Aim and fire entity", ref aimbot);
            ImGui.Checkbox("Aim and fire brother", ref aimOnTeam);
            ImGui.Checkbox("Enable box persepctive", ref enableBoxPerspective);

            if(ImGui.CollapsingHeader("Brothers color"))
            {
                ImGui.ColorPicker4("##brothercolor", ref brotherBoxColor);
            }
            if (ImGui.CollapsingHeader("Enemy color"))
            {
                ImGui.ColorPicker4("##enemycolor", ref enemyBoxColor);
            }
            // 绘制画布
            DrawOverlay(screenSize);
            drawList = ImGui.GetWindowDrawList();
            // 遍历绘制方框透视
            if(enableBoxPerspective)
            {
                foreach (var entity in entities)
                {
                    if (EntityOnScreen(entity))
                    {
                        DrawBox(entity);
                        DrawLine(entity);
                    }
                }
            }
        }

        // 敌人在屏幕区域内
        bool EntityOnScreen(Entity entity)
        {
            if(entity.position2D.X > 0 && entity.position2D.X < screenSize.X 
                && entity.position2D.Y > 0
                && entity.position2D.Y < screenSize.Y)
            {
                return true;
            }
            return false;
        }

        // 画方框盒子
        private void DrawBox(Entity entity)
        {
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;
            Vector2 rectTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);
            Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3, entity.position2D.Y);
            Vector4 boxColor = localPlayer.team == entity.team ? brotherBoxColor : enemyBoxColor;
            drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));
        }

        // 从屏幕底部中央画线
        private void DrawLine(Entity entity)
        {
            Vector4 lineColor = localPlayer.team == entity.team ? brotherBoxColor : enemyBoxColor;
            drawList.AddLine(new Vector2(screenSize.X / 2, screenSize.Y), entity.position2D, ImGui.ColorConvertFloat4ToU32(lineColor));
        }

        public void UpdateEntities(IEnumerable<Entity> newEntities)
        {
            entities = new ConcurrentQueue<Entity>(newEntities);
        }

        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }

        public Entity GetLocalPlayer()
        {
            lock(entityLock)
            {
                return localPlayer;
            }
        }

        // 绘制覆盖层，理解为前端的canvas画布
        void DrawOverlay(Vector2 screenSize)
        {
            ImGui.SetNextWindowSize(screenSize);
            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.Begin("overlay", ImGuiWindowFlags.NoDecoration
                | ImGuiWindowFlags.NoBackground
                | ImGuiWindowFlags.NoBringToFrontOnFocus
                | ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoInputs
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoScrollbar
                | ImGuiWindowFlags.NoScrollWithMouse
                );
        }

    }
}
