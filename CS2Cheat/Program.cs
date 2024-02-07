using System.Runtime.InteropServices;
using System.Numerics;
using Swed64;
using CS2Cheat;
using System.Drawing;

// 初始化内存管理
Swed swed = new Swed("cs2");

// 寻找基址
IntPtr client = swed.GetModuleBase("client.dll");

// 初始化ImGui
Renderer renderer = new Renderer();
// 屏幕尺寸
Vector2 screenSize = renderer.screenSize;
Thread renderThread = new Thread(new ThreadStart(renderer.Start().Wait));
renderThread.Start();


// entity初始化
Entity localPlayer = new Entity();
List<Entity> entities  = new List<Entity>();

const int HOTKEY = 0x05; // 鼠标左下辅助键
const uint STANDING = 65665;// 站立
const uint CROUCHING = 65667;// 蹲伏
const uint PLUS_JUMP = 65537;// +jump
const uint MINUS_JUMP = 256;// -jump
const int PLUS_ATTACK = 65537;// +attack
const int MINUS_ATTACK = 256;// -attack
IntPtr forceJump = client + 0x16CE390;// 跳
IntPtr forceAttack = client + 0x16CDE80;// 攻击

while (true)
{
    //Console.Clear();
    entities.Clear();

    IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);
    // 获取entity列表的控制器
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    // 读取并更新本地玩家的信息
    localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
    localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);
    localPlayer.position = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vOldOrigin);
    localPlayer.viewOffset = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vecViewOffset);

    // 遍历所有的entity
    for (int i = 0; i < 64; i++)
    {
        if (listEntry == IntPtr.Zero) { continue; }
        // 获取当前entity的控制器
        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);
        // 是本地玩家（我）就跳过
        //if (currentController == localPlayer.pawnAddress) { continue; }
        // 获取当前entity的pawnHandle
        int pawnHandle = swed.ReadInt(currentController, Offsets.m_hPlayerPawn);
        if (pawnHandle == 0) { continue; }
        // 获取当前entity的pawn
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
        if (listEntry == IntPtr.Zero) { continue; }
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));
        if (currentPawn == IntPtr.Zero) { continue; }
        uint lifeState = swed.ReadUInt(currentPawn, Offsets.m_lifeState);
        // 当前玩家死亡或未知
        if(lifeState != 256)
        {
            continue;
        }
        // 获取矩阵
        float[] viewMatrix = swed.ReadMatrix(client + Offsets.dwViewMatrix);
        // 读取entity相关数据
        int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
        int health = swed.ReadInt(currentPawn, Offsets.m_iHealth);
        string name = swed.ReadString(currentController, Offsets.m_iszPlayerName, 25);
        //// 队友且设置了不攻击队友
        //if (team == localPlayer.team && !renderer.aimOnTeam)
        //{
        //    continue;
        //}
        // 收集所有敌人的信息
        Entity entity = new Entity();
        entity.pawnAddress = currentPawn;
        entity.name = name;
        entity.health = health;
        entity.lifeState  = lifeState;
        entity.position = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin);
        entity.viewOffset = swed.ReadVec(currentPawn, Offsets.m_vecViewOffset);
        entity.distance = Vector3.Distance(entity.position, localPlayer.position);
        entity.position2D = Calculate.WorldToScreen(viewMatrix, entity.position, screenSize);
        entity.viewPosition2D = Calculate.WorldToScreen(viewMatrix, Vector3.Add(entity.position, entity.viewOffset), screenSize);
        entities.Add(entity);

        //// 控制台打印格式
        //Console.ForegroundColor = ConsoleColor.Yellow;
        //if(team != localPlayer.team)
        //{
        //    Console.ForegroundColor = ConsoleColor.Red;
        //}
        //Console.WriteLine($"Tip · 玩家{name}，还剩 {health} 点血, 距离我 {(int)(entity.distance) /100} 远");
        //Console.ResetColor();
    }

    // 将数据送给渲染线程去渲染
    renderer.UpdateLocalPlayer(localPlayer);
    renderer.UpdateEntities(entities);

    // 按下鼠标辅助键进行开挂
    if(entities.Count > 0 && GetAsyncKeyState(HOTKEY) < 0 && renderer.aimbot)
    {
        entities = entities.OrderBy(o => o.distance).ToList();
        Vector3 playerView = Vector3.Add(localPlayer.position, localPlayer.viewOffset);
        Vector3 entityView = Vector3.Add(entities[0].position, entities[0].viewOffset);

        // 计算真实世界的3d角度转换到游戏中的小地图2d坐标
        Vector2 newAngles = Calculate.CalculateAngles(playerView, entityView);
        Vector3 newAnglesVec3 = new Vector3(newAngles.Y, newAngles.X, 0.0f);
        // 更改游戏的角度等同于更改了玩家的角度
        swed.WriteVec(client, Offsets.dwViewAngles, newAnglesVec3);
        // 射击
        swed.WriteInt(forceAttack, PLUS_ATTACK);
        Thread.Sleep(1);
        swed.WriteInt(forceAttack, MINUS_ATTACK);
    }

    //Thread.Sleep(2);
}

/**
    // 获取本地玩家地址
    IntPtr localPlayerPawn = swed.ReadPointer(client, 0x16D4F48);
    // 获取本地玩家的Flag
    uint fFlag = swed.ReadUInt(localPlayerPawn, 0x3C8);
    // 获取玩家index
    int entIndex = swed.ReadInt(localPlayerPawn, 0x1544);
    Console.WriteLine($"Tip · 已瞄准敌人，敌人ID为：{entIndex} ");
    // 获取玩家的闪光弹持续时间
    float flashDuration = swed.ReadFloat(localPlayerPawn, 0x145C);
    // 获取entity列表地址
    IntPtr entityList = swed.ReadPointer(client, dwEntityList);
    // 获取entity列表的控制器
    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    if (GetAsyncKeyState(SPACE_BAR) < 0)
    {
        // 玩家在地面就跳越
        if(fFlag == STANDING || fFlag == CROUCHING)
        {
            Thread.Sleep(1);
            swed.WriteUInt(forceJump, PLUS_JUMP);
        } else
        {
            swed.WriteUInt(forceJump, MINUS_JUMP);
        }
    }

    // 更改闪光弹的持续时间
    if(flashDuration > 0)
    {
        swed.WriteFloat(localPlayerPawn, 0x145C, 0);
    }
*/

/// 处理按键
[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);