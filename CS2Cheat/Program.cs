using System.Runtime.InteropServices;
using Swed64;

// 初始化内存管理
Swed swed = new Swed("cs2");

const int SPACE_BAR = 0x20; // 空格键

const uint STANDING = 65665;// 站立
const uint CROUCHING = 65667;// 蹲伏
const uint PLUS_JUMP = 65537;// +jump
const uint MINUS_JUMP = 256;// -jump

IntPtr client = swed.GetModuleBase("client.dll");
IntPtr forceJumpAddress = client + 0x16CE390;

while (true)
{
    // 获取本地玩家地址
    IntPtr playerPawnAddress = swed.ReadPointer(client, 0x16D4F48);
    // 获取本地玩家的Flag
    uint fFlag = swed.ReadUInt(playerPawnAddress, 0x3C8);

    if(GetAsyncKeyState(SPACE_BAR) < 0)
    {
        // 玩家在地面就跳越
        if(fFlag == STANDING || fFlag == CROUCHING)
        {
            Thread.Sleep(1);
            swed.WriteUInt(forceJumpAddress, PLUS_JUMP);
        } else
        {
            swed.WriteUInt(forceJumpAddress, MINUS_JUMP);
        }
    }

    Thread.Sleep(5);
}

/// 处理按键
[DllImport("user32.dll")]
static extern short GetAsyncKeyState(int vKey);