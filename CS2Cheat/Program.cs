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

    if(GetAsyncKeyState(SPACE_BAR) < 0)
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