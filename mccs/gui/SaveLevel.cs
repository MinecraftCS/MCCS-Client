namespace MineCS.mccs.gui
{
    public class SaveLevel : LoadLevel
    {
        public SaveLevel(BaseGui oldGui) : base(oldGui)
        {
            label = "Save level";
        }

        public override void loadButtonInfo(string[] worlds)
        {
            for (int i = 0; i < 5; i++)
            {
                menuItems[i].label = worlds[i];
                menuItems[i].isButton = true;
            }
        }

        public override void uponLoading(int worldId)
        {
            client.setGui(new CreateLevel(this, menuItems[worldId].label, worldId));
        }
    }
}
