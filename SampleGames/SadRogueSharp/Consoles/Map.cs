﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RogueSharp;
using SadConsole.Input;
using Microsoft.Xna.Framework.Input;
using SadConsole.Effects;
using Microsoft.Xna.Framework;

namespace SadRogueSharp.Consoles
{
	class MapConsole : SadConsole.Consoles.Console
	{
		protected IMap map;
		protected List<SadConsole.Entities.Entity> entities;
        protected Entities.Player player;
		protected RogueSharp.Random.IRandom random = new RogueSharp.Random.DotNetRandom();

		Recolor explored;
		SadConsole.CellAppearance[,] mapData;

		public MapConsole(int width, int height) : base(width, height)
		{
			// Create the map
			IMapCreationStrategy<Map> mapCreationStrategy = new RandomRoomsMapCreationStrategy<Map>(width, height, 100, 15, 4);
			map = Map.Create(mapCreationStrategy);

			mapData = new SadConsole.CellAppearance[width, height];

            

			foreach (var cell in map.GetAllCells())
			{
				if (cell.IsWalkable)
				{
					// Our local information about each map square
					mapData[cell.X, cell.Y] = new MapObjects.Floor();
					// Copy the appearance we've defined for Floor or Wall or whatever, to the actual console data that is rendered
					mapData[cell.X, cell.Y].CopyAppearanceTo(_cellData[cell.X, cell.Y]);
				}
				else
				{
					mapData[cell.X, cell.Y] = new MapObjects.Wall();
					mapData[cell.X, cell.Y].CopyAppearanceTo(_cellData[cell.X, cell.Y]);
				}
			}

			// Create map effects
			explored = new SadConsole.Effects.Recolor();
			explored.Background = Color.White * 0.3f;
			explored.Foreground = Color.White * 0.3f;
			explored.Update(10d); // Trickery to force the fade to complete to the destination color.

			// Entities
			entities = new List<SadConsole.Entities.Entity>();
			
			// Create the player
			player = new Entities.Player();
			var tempCell = GetRandomEmptyCell();
			player.Position = new Microsoft.Xna.Framework.Point(tempCell.X, tempCell.Y);
			entities.Add(player);
            
            // Create a hound
            GenerateHound();
			GenerateHound();
			GenerateHound();

			// Initial view
			UpdatePlayerView();

			// Keyboard setup
			SadConsole.Engine.Keyboard.RepeatDelay = 0.07f;
			SadConsole.Engine.Keyboard.InitialRepeatDelay = 0.1f;
		}

		private void GenerateHound()
		{
			var hound = new Entities.Hound();
			hound.PositionOffset = this.Position;
			var tempCell = GetRandomEmptyCell();
			hound.Position = new Microsoft.Xna.Framework.Point(tempCell.X, tempCell.Y);
			entities.Add(hound);
		}

		public override void Update()
		{
			// Normally just the console data for this console is "updated" each frame, 
			// but we want to also update all entities.
			foreach (var entity in entities)
				entity.Update();

			base.Update();
		}


		public override void Render()
		{
			
			base.Render();

			// Normally only the console data is rendered through this call, but after
			// that has finished, we want to render our entities on top of that.
			foreach (var entity in entities)
				entity.Render();
		}

		public override bool ProcessKeyboard(KeyboardInfo info)
		{
			bool keyHit = false;

			var newPosition = player.Position;

			if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Up)))
			{
				newPosition.Y -= 1;
				keyHit = true;
			}
			else if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Down)))
			{
				newPosition.Y += 1;
				keyHit = true;
			}

			if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Left)))
			{
				newPosition.X -= 1;
				keyHit = true;
			}
			else if (info.KeysPressed.Contains(AsciiKey.Get(Keys.Right)))
			{
				newPosition.X += 1;
				keyHit = true;
			}

			// Test location
			if (map.IsWalkable(newPosition.X, newPosition.Y))
			{
				player.Position = newPosition;

				UpdatePlayerView();

			}

			return keyHit || base.ProcessKeyboard(info);

		}

		private void UpdatePlayerView()
		{
			// Find out what the player can see
			map.ComputeFov(player.Position.X, player.Position.Y, 20, true);

			// Mark all render points as visible or not
			for (int i = 0; i < _cellData.CellCount; i++)
			{
				var point = _cellData.GetPointFromIndex(i);
				var currentCell = map.GetCell(point.X, point.Y);

				if (currentCell.IsInFov)
				{
					if (_cellData[i].Effect != null)
					{
						explored.Clear(_cellData[i]);
						_cellData[i].Effect = null;
					}

					_cellData[i].IsVisible = true;
					map.SetCellProperties(point.X, point.Y, currentCell.IsTransparent, currentCell.IsWalkable, true);
				}
				else if (currentCell.IsExplored)
				{
					_cellData[i].IsVisible = true;
					_cellData[i].Effect = explored;
					explored.Apply(_cellData[i]);
				}
				else
				{
					_cellData[i].IsVisible = false;
				}
			}

            // Calculate the view area and sync it with our player location
            ViewArea = new Microsoft.Xna.Framework.Rectangle(player.Position.X - 15, player.Position.Y - 15, 30, 30);
            player.PositionOffset = this.Position - ViewArea.Location;

            
            // Check for entities.
            foreach (var entity in entities)
			{
                if (entity != player)
                {
                    entity.IsVisible = map.IsInFov(entity.Position.X, entity.Position.Y);
                    
                    // Entity is in our view, but it may not be within the viewport.
                    if (entity.IsVisible)
                    {
                        entity.PositionOffset = this.Position - ViewArea.Location;

                        // If the entity is not in our view area we don't want to show it.
                        if (!ViewArea.Contains(entity.Position))
                            entity.IsVisible = false;
                    }

                }
			}

		}

		private Cell GetRandomEmptyCell()
		{

			while (true)
			{
				int x = random.Next(_cellData.Width - 1);
				int y = random.Next(_cellData.Height - 1);
				if (map.IsWalkable(x, y))
				{
					return map.GetCell(x, y);
				}
			}

		}

	}
}
