using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace CS2Cheat
{
    public class Renderer : Overlay
    {
        public bool aimbot = true;
        public bool aimOnTeam = false;

        protected override void Render()
        {
            ImGui.Begin("CS2 Cheat by C#");

            ImGui.Checkbox("Aim and fire entity", ref aimbot);
            ImGui.Checkbox("Aim and fire brother", ref aimOnTeam);
        }


    }
}
