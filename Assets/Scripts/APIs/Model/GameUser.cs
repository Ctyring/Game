using cn.bmob.io;

namespace Model
{
    public class GameUser : BmobUser
    {
        public override void write(BmobOutput output, bool all)
        {
            base.write(output, all);
        }

        public override void readFields(BmobInput input)
        {
            base.readFields(input);
        }
    }
}