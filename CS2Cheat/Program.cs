using System.Runtime.InteropServices;
using Swed64;

// 初始化内存管理
Swed swed = new Swed("cs2");

const int SPACE_BAR = 0x20; // 空格键

const uint STANDING = 65665;// 站立
const uint CROUCHING = 65667;// 蹲伏
const uint PLUS_JUMP = 65537;// +jump
const uint MINUS_JUMP = 256;// -jump
const int PLUS_ATTACK = 65537;// +attack
const int MINUS_ATTACK = 256;// -attack

IntPtr client = swed.GetModuleBase("client.dll");
IntPtr forceJump = client + 0x16CE390;
IntPtr forceAttack = client + 0x16CDE80;

int dwEntityList = 0x17CE6A0;
int m_hPlayerPawn = 0x7EC;
int m_iHealth = 0x32C;
int m_iszPlayerName = 0x640;


while (true)
{
    Console.Clear();
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

    for(int i=0; i < 64; i++)
    {
        if(listEntry == IntPtr.Zero) { continue; }
        // 获取当前entity的控制器
        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);
        if (currentController == IntPtr.Zero) { continue; }
        // 获取当前entity的pawnHandle
        int pawnHandle = swed.ReadInt(currentController, m_hPlayerPawn);
        if (pawnHandle == 0) { continue; }
        // 获取当前entity的pawn
        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));
        // 读取entity相关数据
        uint health = swed.ReadUInt(currentPawn, m_iHealth);
        string name = swed.ReadString(currentController, m_iszPlayerName, 25);

        Console.WriteLine($"Tip · 玩家{name}当前剩余血量: {health}");
    }

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

    if( GetAsyncKeyState(0x5) < 0)
    {
        // 如果有敌人在准星前就开枪
        if(entIndex > 0)
        {
            swed.WriteInt(forceAttack,  PLUS_ATTACK);
            Thread.Sleep(1);
            swed.WriteInt(forceAttack,  MINUS_ATTACK);
        }
    }



    // 更改闪光弹的持续时间
    if(flashDuration > 0)
    {
        swed.WriteFloat(localPlayerPawn, 0x145C, 0);
    }

    Thread.Sleep(2);
}

/// 处理按键
[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);