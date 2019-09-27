﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using IDx3DSharp.DemoApp.Demos;

namespace IDx3DSharp.DemoApp
{
	public partial class MainForm : Form
	{
        readonly Scene _scene;
        readonly bool _initialized;
        bool _antialias;

        int _oldx;
        int _oldy;
        bool _autorotation = true;

		public MainForm(BaseDemo demo)
		{
			InitializeComponent();

			SetStyle(
				ControlStyles.AllPaintingInWmPaint |
				ControlStyles.UserPaint |
				ControlStyles.DoubleBuffer, true);

			Size = new Size(800, 600);
			Text = "IDx3DSharp Demos";

			// BUILD SCENE

			_scene = new Scene(Width, Height);
			demo.PopulateScene(_scene);

			_initialized = true;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.DrawImageUnscaled(GetImage(), 0, 0);
			base.OnPaint(e);
		}

		public void UpdateScene()
		{
			lock (this)
			{
				if (!_initialized) return;
				if (_autorotation)
				{
					float speed = 1;
					var dx = (float) Math.Sin((float) Environment.TickCount / 1000) / 20;
					var dy = (float) Math.Cos((float) Environment.TickCount / 1000) / 20;
					_scene.rotate(-speed * dx, speed * dy, speed * 0.04f);
				}
				_scene.render();
			}
		}

		public Image GetImage()
		{
			return _scene.getImage();
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			_oldx = e.X;
			_oldy = e.Y;
			SetMovingCursor();
			_autorotation = false;

			base.OnMouseDown(e);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F) { Console.WriteLine(_scene.getFPS() + ""); e.Handled = true; }
			if (e.KeyCode == Keys.PageUp) { _scene.shift(0f, 0f, 0.2f); e.Handled = true; }
			if (e.KeyCode == Keys.PageDown) { _scene.shift(0f, 0f, -0.2f); e.Handled = true; }
			if (e.KeyCode == Keys.Up) { _scene.shift(0f, -0.2f, 0f); e.Handled = true; }
			if (e.KeyCode == Keys.Down) { _scene.shift(0f, 0.2f, 0f); e.Handled = true; }
			if (e.KeyCode == Keys.Left) { _scene.shift(0.2f, 0f, 0f); e.Handled = true; }
			if (e.KeyCode == Keys.Right) { _scene.shift(-0.2f, 0f, 0f); e.Handled = true; }
			if ((char) e.KeyValue == 'a') { _antialias = !_antialias; _scene.setAntialias(_antialias); e.Handled = true; }
			if ((char) e.KeyValue == '+') { _scene.scale(1.2f); e.Handled = true; }
			if ((char) e.KeyValue == '-') { _scene.scale(0.8f); e.Handled = true; }

			base.OnKeyDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (!_autorotation)
			{
				var dx = (float) (e.Y - _oldy) / 50;
				var dy = (float) (_oldx - e.X) / 50;
				_scene.rotate(dx, dy, 0);
				_oldx = e.X;
				_oldy = e.Y;
			}

			base.OnMouseMove(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			_autorotation = true;
			SetNormalCursor();

			base.OnMouseUp(e);
		}

        void SetMovingCursor()
		{
			Cursor = Cursors.SizeAll;
		}

        void SetNormalCursor()
		{
			Cursor = Cursors.Hand;
		}

		protected override void OnResize(EventArgs e)
		{
            _scene?.resize(Size.Width, Size.Height);
            base.OnResize(e);
		}
	}
}