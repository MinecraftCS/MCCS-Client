namespace MineCS.mccs.gui
{
    public class PauseMenu : BaseGui
    {
        public override void loadGui()
        {
            menuItems.Clear();
            menuItems.Add(new MenuItem(0, screenWidth / 2 - 100, screenHeight / 3, 200, 20, "Generate new level"));
            menuItems.Add(new MenuItem(1, screenWidth / 2 - 100, screenHeight / 3 + 32, 200, 20, "Save level.."));
            menuItems.Add(new MenuItem(2, screenWidth / 2 - 100, screenHeight / 3 + 64, 200, 20, "Load level.."));
            menuItems.Add(new MenuItem(3, screenWidth / 2 - 100, screenHeight / 3 + 96, 200, 20, "Back to game"));
            if (client.session == null)
            {
                menuItems[1].active = false;
                menuItems[2].active = false;
            }
        }

        public override void btnClicked(MenuItem btn)
        {
            if (btn.id == 0)
            {
                client.generateWorld();
                client.setGui(null);
                client.grabMouse();
            }
            if (client.session != null)
            {
                if (btn.id == 1)
                    client.setGui(new SaveLevel(this));
                else if (btn.id == 2)
                    client.setGui(new LoadLevel(this));
            }
            if (btn.id == 3)
            {
                client.setGui(null);
                client.grabMouse();
            }
        }

        public override void render(int mouseX, int mouseY)
        {
            renderBackground(0, 0, screenWidth, screenHeight, 0x60050500, 0xA0303060);
            renderTextFromRight("Game menu", screenWidth / 2, 40, 0xFFFFFF);
            base.render(mouseX, mouseY);
        }
    }
}
