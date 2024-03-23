using OpenTK.Input;

namespace MineCS.mccs.gui
{
    public class CreateLevel : BaseGui
    {
        private BaseGui oldGui;
        private string label = "Enter level name:";
        private int worldId;
        private string editBox;
        private int cursorBlink = 0;

        public CreateLevel(BaseGui oldGui, string worldName, int worldId)
        {
            this.oldGui = oldGui;
            this.worldId = worldId;
            editBox = worldName;
            if (editBox == "-")
                editBox = string.Empty;
        }

        public override void loadGui()
        {
            menuItems.Clear();
            menuItems.Add(new MenuItem(0, screenWidth / 2 - 100, screenHeight / 4 + 120, 200, 20, "Save"));
            menuItems.Add(new MenuItem(1, screenWidth / 2 - 100, screenHeight / 4 + 144, 200, 20, "Cancel"));
            menuItems[0].active = editBox.Trim().Length > 1;
        }

        public override void refresh() { }

        public override void advanceTime()
        {
            cursorBlink++;
        }

        public override void btnClicked(MenuItem btn)
        {
            if (!btn.active) return;
            if (btn.id == 0 && editBox.Length > 1)
            {
                string worldName = editBox.Trim();
                client.levelOnline.saveLevelOnline(client.level, client.serverHost, client.session.username, client.session.sessionId, worldName, worldId);
                client.setGui(null!);
            }
            if (btn.id == 1)
                client.setGui(oldGui);
        }

        public override void keyPressed(char keyChar, Key keyId)
        {
            if (keyId == Key.BackSpace)
                editBox = editBox.Substring(0, editBox.Length - 1);
            if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ,.:-_'*!\\\"#%/()=+?[]{}<>".Contains(keyChar))
                editBox += keyChar;
            menuItems[0].active = editBox.Trim().Length > 1;
        }

        public override void render(int mouseX, int mouseY)
        {
            renderBackground(0, 0, screenWidth, screenHeight, 0x60050500, 0xA0303060);
            renderTextFromRight(label, screenWidth / 2, 40, 0xFFFFFF);
            int x = screenWidth / 2 - 100;
            int y = screenHeight / 2 - 10;
            drawButton(x - 1, y - 1, x + 200 + 1, y + 20 + 1, 0xFFA0A0A0);
            drawButton(x, y, x + 200, y + 20, 0xFF000000);
            renderText(editBox + (cursorBlink / 6 % 2 == 0 ? "_" : ""), x + 4, y + 6, 0xE0E0E0);
            base.render(mouseX, mouseY);
        }
    }
}
