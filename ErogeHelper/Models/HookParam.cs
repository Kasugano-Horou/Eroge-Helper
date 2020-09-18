using System;

namespace ErogeHelper.Models
{
    //                           diff
    //[Handle:ProcessId:Address :Context:Context2:Name(Engine):HookCode                 ]
    //[19    :272C     :769550C0:2C78938:0       :TextOutA    :HS10@0:gdi32.dll:TextOutA] 俺は…………。
    class HookParam : EventArgs
    {
        public long handle { get; set; }
        public long pid { get; set; }
        public long addr { get; set; }
        public long ctx { get; set; }
        public long ctx2 { get; set; }
        public string name { get; set; }
        public string hookcode { get; set; }
        public string text { get; set; }

        public string Info()
        {
            return name + ':' + hookcode;
        }
    }
}
