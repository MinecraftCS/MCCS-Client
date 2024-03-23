namespace MineCS.mccs.gui
{
    public class MenuItem
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public string label;
        public int id;
        public bool active = true;
        public bool isButton = true;

        public MenuItem(int id, int x, int y, int width, int height, string label)
        {
            this.id = id;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.label = label;
        }
    }
}
