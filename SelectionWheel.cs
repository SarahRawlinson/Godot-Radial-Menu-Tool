using System;
using System.Collections.Generic;
using Godot;

namespace chess.UI.Common;

[Tool]
public partial class SelectionWheel : Control
{
	[ExportGroup("Colours")]
	[Export] private Color _innerColor = Colors.Aqua;
	[Export] private Color _outerColor = Colors.Blue;
	[Export] private Color _lineColor = Colors.Crimson;
	[Export] private Color _highlightColor = Colors.Beige;

	[ExportGroup("Sizing")]
	[Export] private int _outerRadius = 256;
	[Export] private int _innerRadius = 64;
	[Export] private int _lineWidth = 4;
	
	[ExportGroup("Mode")]
	[Export] public bool ShowWheel = true;
	[Export] public bool MouseMode = false;
	[Export] public bool DisplayActivator = true;

	[ExportSubgroup("Sprites")]
	[Export] private Vector2 _spriteSize = new Vector2(125, 125);

	[ExportGroup("Options")]
	[Export] private WheelOption[] _options;

	private int _selection = -1;
	public event Action<WheelOption> OnSelected;
	

	public override void _Ready()
	{
		OnSelected += option => { GD.Print($"Selected {option.Name}"); };
	}

	public void SetOptions(WheelOption[] options)
	{
		_options = options;
	}
	
	public override void _Process(double delta)
	{
		if (ShowWheel && (Engine.IsEditorHint() || !DisplayActivator || Input.IsActionPressed("ShowWheel")))
		{
			var mousePos = GetLocalMousePosition();
			var mouseRadius = mousePos.Length();
			var joypadRadius = 0f;
			Vector2 joypadPos = new Vector2();

			if (!Engine.IsEditorHint())
			{
				joypadPos = Input.GetVector("SelectWheelLeft", "SelectWheelRight", "SelectWheelUp", "SelectWheelDown");
				joypadRadius = joypadPos.Length();
			}
			if ((MouseMode && mouseRadius < _innerRadius) || (Engine.IsEditorHint() && !MouseMode) ||
				(!MouseMode && joypadRadius < 0.2f))
			{
				_selection = -1;
			}
			else
			{
				var mouseRads = Mathf.PosMod(mousePos.Angle() * -1, Mathf.Tau);
				var joypadRads = Mathf.PosMod(joypadPos.Angle() * -1, Mathf.Tau);
				if (MouseMode) _selection = (int) Mathf.Ceil((mouseRads / Mathf.Tau) * _options.Length);
				else _selection = (int) Mathf.Ceil((joypadRads / Mathf.Tau) * _options.Length);
				if (_selection == _options.Length) _selection = 0;
				if (!Engine.IsEditorHint() && Input.IsActionJustPressed("SelectWheel"))
				{
					OnSelected?.Invoke(_options[_selection]);
				}
			}
		}
		QueueRedraw();
	}

	public override void _Draw()
	{
		
		if (ShowWheel && (Engine.IsEditorHint() || !DisplayActivator || Input.IsActionPressed("ShowWheel")))
		{
			var offset = _spriteSize / -2;
			DrawCircle(Vector2.Zero, _outerRadius, _outerColor);
			DrawCircle(Vector2.Zero, _innerRadius, _innerColor);
			DrawArc(Vector2.Zero, _innerRadius, 0, Mathf.Tau, 128, _lineColor, _lineWidth, true);
			for (int op = 0; op < _options.Length; op++)
			{
				
				float startRads = (Mathf.Tau * (op - 1)) / _options.Length;
				float endRads = (Mathf.Tau * op) / _options.Length;
				float midRads = (startRads + endRads) / 2.0f * -1;
				float midRadius = (_innerRadius + _outerRadius) / 2.0f;
				var drawPos = midRadius * Vector2.FromAngle(midRads) + offset;
				var point = Vector2.FromAngle(endRads);
				
				if (_selection == op)
				{
					var pointsPerArc = 32;
					List<Vector2> pointsInner = new();
					List<Vector2> pointsOuter = new();

					for (int p = 0; p < pointsPerArc; p++)
					{
						var angle = startRads + p * (endRads - startRads) / pointsPerArc;
						pointsInner.Add(_innerRadius * Vector2.FromAngle(Mathf.Tau - angle));
						pointsOuter.Add(_outerRadius * Vector2.FromAngle(Mathf.Tau - angle));
					}
					pointsOuter.Reverse();
					pointsInner.AddRange(pointsOuter);
					DrawPolygon(pointsInner.ToArray(), new []{_highlightColor});
				}
				DrawLine(point*_innerRadius, point*_outerRadius, _lineColor, _lineWidth, true);
				DrawTextureRectRegion(_options[op].Atlas, new Rect2(drawPos, _spriteSize),  _options[op].Region);
				
			}
		}
	}
}
