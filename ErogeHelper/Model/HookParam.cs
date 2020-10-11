using System;

namespace ErogeHelper.Model
{
    //[Handle:ProcessId:Address :Context:Context2:Name(Engine):HookCode                 ]
    //[19    :272C     :769550C0:2C78938:0       :TextOutA    :HS10@0:gdi32.dll:TextOutA] 俺は…………。
    public class HookParam : EventArgs
    {
        public long Handle { get; set; }
        public long Pid { get; set; }
        public long Addr { get; set; }
        public long Ctx { get; set; }
        public long Ctx2 { get; set; }
        public string Name { get; set; }
        public string Hookcode { get; set; }
        public string Text { get; set; }
    }
}
