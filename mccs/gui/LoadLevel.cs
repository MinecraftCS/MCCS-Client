namespace MineCS.mccs.gui
{
    public class LoadLevel : BaseGui
    {
        private BaseGui oldGui;
        private bool loaded = false;
        private bool loadedWorlds = false;
        private string[] worlds = null;
        private string extraLabel = string.Empty;
        protected string label = "Load level";

        public LoadLevel(BaseGui oldGui)
        {
            this.oldGui = oldGui;
        }

        public void run()
        {
            extraLabel = "Failed to load levels";
            loaded = true;
        }

        public virtual void loadButtonInfo(string[] worlds)
        {
            for (int i = 0; i < 5; i++)
            {
                menuItems[i].active = worlds[i] != "-";
                menuItems[i].label = worlds[i];
                menuItems[i].isButton = true;
            }
        }

        public override void loadGui()
        {
            run();
            menuItems.Clear();
            for (int i = 0; i < 5; i++)
            {
                menuItems.Add(new MenuItem(i, screenWidth / 2 - 100, screenHeight / 4 + i * 24, 200, 20, "-"));
                menuItems[i].isButton = false;
            }
            menuItems.Add(new MenuItem(5, screenWidth / 2 - 100, screenHeight / 4 + 144, 200, 20, "Cancel"));
        }

        public override void btnClicked(MenuItem btn)
        {
            if (!btn.active) return;
            if (loadedWorlds && btn.id < 5)
                uponLoading(btn.id);
            if (loaded || loadedWorlds && btn.id == 5)
                client.setGui(oldGui);
        }

        public virtual void uponLoading(int worldId)
        {
            client.loadLevel(client.session.username, worldId);
            client.setGui(null!);
            client.grabMouse();
        }

        public override void render(int mouseX, int mouseY)
        {
            renderBackground(0, 0, screenWidth, screenHeight, 0x60050500, 0xA0303060);
            renderTextFromRight(label, screenWidth / 2, 40, 0xFFFFFF);
            if (!loadedWorlds)
                renderTextFromRight(extraLabel, screenWidth / 2, screenHeight / 2 - 4, 0xFFFFFF);
            base.render(mouseX, mouseY);
        }
    }
}
