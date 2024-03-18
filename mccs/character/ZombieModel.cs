namespace MineCS.mccs.character
{
    public class ZombieModel
    {
        public Cube head;
        public Cube body;
        public Cube arm0;
        public Cube arm1;
        public Cube leg0;
        public Cube leg1;

        public ZombieModel()
        {
            head = new Cube(0, 0);
            head.addBox(-4.0f, -8.0f, -4.0f, 8, 8, 8);
            body = new Cube(16, 16);
            body.addBox(-4.0f, 0.0f, -2.0f, 8, 12, 4);
            arm0 = new Cube(40, 16);
            arm0.addBox(-3.0f, -2.0f, -2.0f, 4, 12, 4);
            arm0.setPos(-5.0f, 2.0f, 0.0f);
            arm1 = new Cube(40, 16);
            arm1.addBox(-1.0f, -2.0f, -2.0f, 4, 12, 4);
            arm1.setPos(5.0f, 2.0f, 0.0f);
            leg0 = new Cube(0, 16);
            leg0.addBox(-2.0f, 0.0f, -2.0f, 4, 12, 4);
            leg0.setPos(-2.0f, 12.0f, 0.0f);
            leg1 = new Cube(0, 16);
            leg1.addBox(-2.0f, 0.0f, -2.0f, 4, 12, 4);
            leg1.setPos(2.0f, 12.0f, 0.0f);
        }

        public void render(float time)
        {
            head.yRot = (float)Math.Sin(time * 0.83);
            head.xRot = (float)Math.Sin(time) * 0.8f;
            arm0.xRot = (float)Math.Sin(time * 0.6662 + Math.PI) * 2.0f;
            arm0.zRot = (float)(Math.Sin(time * 0.2312) + 1.0);
            arm1.xRot = (float)Math.Sin(time * 0.6662) * 2.0f;
            arm1.zRot = (float)(Math.Sin(time * 0.2812) - 1.0);
            leg0.xRot = (float)Math.Sin(time * 0.6662) * 1.4f;
            leg1.xRot = (float)Math.Sin(time * 0.6662 + Math.PI) * 1.4f;
            head.render();
            body.render();
            arm0.render();
            arm1.render();
            leg0.render();
            leg1.render();
        }
    }
}
