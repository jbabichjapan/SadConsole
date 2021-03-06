﻿namespace StarterProject.CustomConsoles
{
    using System;
    using SadConsole;
    using SadConsole.Consoles;
    using Console = SadConsole.Consoles.Console;
    using Microsoft.Xna.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework.Graphics;
    using SadConsole.Instructions;

    class SplashScreen: Console
    {

        public Action SplashCompleted { get; set; }

        private InstructionSet _animation;
        private CellsRenderer _consoleImage;

        int _x = -50;

        public SplashScreen()
            : base(80, 25)
        {
            IsVisible = false;

            // Setup the console text background
            string textTemplate = "sole SadCon";
            System.Text.StringBuilder text = new System.Text.StringBuilder(2200);

            for (int i = 0; i < CellData.Width * CellData.Height; i++)
            {
                text.Append(textTemplate);
            }
            this.CellData.Print(0, 0, text.ToString(), Color.Black, Color.Transparent);

            // Load the logo
            System.IO.Stream imageStream = System.IO.File.OpenRead("sad.png");
            var image = Texture2D.FromStream(Engine.Device, imageStream);
            imageStream.Dispose();

            // Configure the logo
            _consoleImage = new CellsRenderer(new CellSurface(image.Width, image.Height), this.Batch);
            _consoleImage.Position = new Point(CellData.Width / 2 - image.Width / 2, -1);
            image.DrawImageToSurface(_consoleImage.CellData, new Point(0,0), true);
            _consoleImage.Tint = Color.Black;

            // Configure the animations
            _animation = new InstructionSet();
            _animation.Instructions.AddLast(new Wait() { Duration = 0.3f });

            // Animation to move the angled gradient spotlight effect.
            var moveGradientInstruction = new CodeInstruction();
            moveGradientInstruction.CodeCallback = (inst) =>
                {
                    _x += 1;

                    if (_x > _cellData.Width + 50)
                    {
                        inst.IsFinished = true;
                    }

                    Color[] colors = new Color[] { Color.Black, Color.DarkBlue, Color.White, Color.DarkBlue, Color.Black };
                    float[] colorStops = new float[] { 0f, 0.2f, 0.5f, 0.8f, 1f };

                    Algorithms.GradientFill(CellSize, new Point(_x, 12), 10, 45, new Rectangle(0, 0, CellData.Width, CellData.Height), new ColorGradient(colors, colorStops), _cellData.SetForeground);
                };
            _animation.Instructions.AddLast(moveGradientInstruction);

            // Animation to clear the SadConsole text.
            _animation.Instructions.AddLast(new CodeInstruction() { CodeCallback = (i) => { _cellData.Fill(Color.Black, Color.Transparent, 0, null); i.IsFinished = true; } });

            // Animation for the logo text.
            var logoText = new ColorGradient(new Color[] { Color.Purple, Color.Yellow }, new float[] { 0.0f, 1f }).ToColoredString("[| Powered by SadConsole |]");
            logoText.SetEffect(new SadConsole.Effects.Fade() { DestinationForeground = Color.Blue, FadeForeground = true, FadeDuration = 1f, Repeat = false, RemoveOnFinished = true, Permanent = true, CloneOnApply = true });
            _animation.Instructions.AddLast(new DrawString(this) { Position = new Point(26, this.CellData.Height - 1), Text = logoText, TotalTimeToPrint = 1f, UseConsolesCursorToPrint = false });

            // Animation for fading in the logo picture.
            _animation.Instructions.AddLast(new FadeCellRenderer(_consoleImage, new ColorGradient(Color.Black, Color.Transparent), new TimeSpan(0, 0, 0, 0, 2000)));

            // Animation to blink SadConsole in the logo text
            _animation.Instructions.AddLast(new CodeInstruction()
            {
                CodeCallback = (i) =>
                    {
                        SadConsole.Effects.Fade fadeEffect = new SadConsole.Effects.Fade();
                        fadeEffect.AutoReverse = true;
                        fadeEffect.DestinationForeground = new ColorGradient(Color.Blue, Color.Yellow);
                        fadeEffect.FadeForeground = true;
                        fadeEffect.Repeat = true;
                        fadeEffect.FadeDuration = 0.7f;

                        List<Cell> cells = new List<Cell>();
                        for (int index = 0; index < 10; index++)
                        {
                            var point = new Point(26, this.CellData.Height - 1).ToIndex(this.CellData.Width) + 14 + index;
                            cells.Add(_cellData[point]);
                        }

                        _cellData.SetEffect(cells, fadeEffect);
                        i.IsFinished = true;
                    }
            });
            
            // Animation to delay, keeping the logo and all on there for 2 seconds, then destroy itself.
            _animation.Instructions.AddLast(new Wait() { Duration = 2.5f });
            _animation.Instructions.AddLast(new FadeCellRenderer(this, new ColorGradient(Color.Transparent, Color.Black), new TimeSpan(0, 0, 0, 0, 2000)));
            _animation.Instructions.AddLast(new CodeInstruction()
            {
                CodeCallback = (i) =>
                {
                    if (this.Parent != null)
                        this.Parent.Remove(this);

                    if (SplashCompleted != null)
                        SplashCompleted();
                }
            });
        }

        public override void Update()
        {
            if (!IsVisible)
                return;

            base.Update();
            
            _animation.Run();
        }

        public override void Render()
        {
            // Draw the logo console...
            if (IsVisible)
                _consoleImage.Render();

            base.Render();
        }
    }
}
