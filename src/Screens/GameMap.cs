// CivOne
//
// To the extent possible under law, the person who associated CC0 with
// CivOne has waived all copyright and related or neighboring rights
// to CivOne.
//
// You should have received a copy of the CC0 legalcode along with this
// work. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CivOne.Enums;
using CivOne.GFX;
using CivOne.Interfaces;
using CivOne.Templates;

namespace CivOne.Screens
{
	internal class GameMap : BaseScreen
	{
		private struct RenderTile
		{
			public int X, Y;
			public ITile Tile;
			public Bitmap Image
			{
				get
				{
					return Resources.Instance.GetTile(Tile);
				}
			}
			public Point Position
			{
				get
				{
					return new Point(X * 16, Y * 16);
				}
			}
		}
		
		private readonly Color[] _palette;
		private bool _update = true;
		private int _x, _y;
		
		private IEnumerable<RenderTile> RenderTiles
		{
			get
			{
				for (int x = 0; x < 15; x++)
				for (int y = 0; y < 12; y++)
				{
					int tx = _x + x;
					int ty = _y + y;
					while (tx >= Map.WIDTH) tx -= Map.WIDTH;
					
					yield return new RenderTile
					{
						X = x,
						Y = y,
						Tile = Map.Instance.GetTile(tx, ty)
					};
				}
			}
		}
		
		public override bool HasUpdate(uint gameTick)
		{
			if (_update)
			{
				_canvas = new Picture(240, 192, _palette);
				foreach (RenderTile t in RenderTiles)
				{
					AddLayer(t.Image, t.Position);
				}
				
				foreach (RenderTile t in RenderTiles.Reverse())
				{
					City city = Game.Instance.GetCity(t.Tile.X, t.Tile.Y);
					if (city == null) continue;
					
					_canvas.FillRectangle(15, t.Position.X + 1, t.Position.Y + 1, 14, 14);
					_canvas.FillRectangle(Common.ColourDark[city.Owner], t.Position.X + 2, t.Position.Y + 1, 13, 13);
					_canvas.FillRectangle(Common.ColourLight[city.Owner], t.Position.X + 2, t.Position.Y + 2, 12, 12);
					
					Bitmap resource = (Bitmap)Resources.Instance.GetPart("SP257", 192, 112, 16, 16).Clone();
					Picture.ReplaceColours(resource, 3, 0);
					Picture.ReplaceColours(resource, 5, Common.ColourDark[city.Owner]);
					AddLayer(resource, t.Position);
					_canvas.DrawText(city.Size.ToString(), 0, 5, 5, t.Position.X + 9, t.Position.Y + 5, TextAlign.Center);
					
					if (t.Y == 11) continue;
					int labelX = (t.X == 0) ? t.Position.X : t.Position.X - 8;
					int labelY = t.Position.Y + 16;
					_canvas.DrawText(city.Name, 0, 5, labelX, labelY + 1, TextAlign.Left);
					_canvas.DrawText(city.Name, 0, 11, labelX, labelY, TextAlign.Left);
				}
				
				_update = false;
				return true;
			}
			
			return false;
		}
		
		public override bool MouseDown(MouseEventArgs args)
		{
			int x = (int)Math.Floor((float)args.X / 16);
			int y = (int)Math.Floor((float)args.Y / 16);
						
			if (args.Button == MouseButtons.Left)
			{
				_x += x - 8;
				_y += y - 6;
				while (_x < 0) _x += Map.WIDTH;
				while (_x >= Map.WIDTH) _x -= Map.WIDTH;
				while (_y < 0) _y++;
				while (_y + 12 > Map.HEIGHT) _y--;
				_update = true;
			}
			return _update;
		}
		
		public GameMap()
		{
			_x = 0;
			_y = 0;
			
			_palette = Resources.Instance.LoadPIC("SP257").Image.Palette.Entries;
		}
	}
}