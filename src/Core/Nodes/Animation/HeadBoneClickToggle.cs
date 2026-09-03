using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace SilentSkinMod.Core.Nodes.Animation;

[GlobalClass]
[GodotClassName("HeadBoneClickToggle")]
public partial class HeadBoneClickToggle : Control
{
	private readonly string[] _slotsToToggle = new string[]
	{
		"tougu mianju 0",
		"tougu mianju 1",
		"tougu mianju 2",
		"mianju yanjing 0",
		"mianju yanjing 6",
		"mianju yanjing 2",
		"mianju yanjing 3",
		"mianju yanjing 4",
		"mianju yanjing 5",
		"mianju yanjing 1",
		"mianju yinying 0",
		"jio_R",
		"mianjv",
        "zuojio"
	};

	private bool _isHidden = false;

	[Export] public Node[] TargetSpineSprites = new Node[0];
	
	private List<GodotObject> _allTargetSlots = new List<GodotObject>();
	private Dictionary<string, Color> _storedColors = new Dictionary<string, Color>();

	public override void _Ready()
	{
		if (TargetSpineSprites == null || TargetSpineSprites.Length == 0)
		{
			var defaultNode = GetNode("../SpineSprite");
			if (defaultNode != null)
			{
				TargetSpineSprites = new Node[] { defaultNode };
			}
			else
			{
				GD.PrintErr("未指定任何 SpineSprite 节点，且无法自动找到");
				return;
			}
		}
		
		foreach (var spineNode in TargetSpineSprites)
		{
			if (spineNode == null) continue;

			// 获取 MegaSkeleton
			var spineSprite = new MegaSprite(Variant.From(spineNode));
			var skeleton = spineSprite.GetSkeleton();
			if (skeleton == null)
			{
				GD.PrintErr($"节点 {spineNode.Name} 无法获取 MegaSkeleton");
				continue;
			}
			
			var slots = skeleton.BoundObject.Call("get_slots");
			if (slots.VariantType == Variant.Type.Array)
			{
				var slotsArray = slots.As<Godot.Collections.Array>();
				foreach (var slotVariant in slotsArray)
				{
					if (slotVariant.VariantType != Variant.Type.Object)
						continue;

					var slot = slotVariant.AsGodotObject();
					string slotName = slot.Call("get_data").AsGodotObject().Call("get_name").AsString();

					if (IsInToggleList(slotName))
					{
						_allTargetSlots.Add(slot);
					}
				}
			}
			
			if (spineNode.HasSignal("world_transforms_changed"))
			{
				spineNode.Connect("world_transforms_changed", new Callable(this, nameof(OnWorldTransformsChanged)));
			}
			else
			{
				GD.PrintErr($"节点 {spineNode.Name} 没有 world_transforms_changed 信号");
			}
		}
		
		ApplyVisibility();
	}

	private void OnWorldTransformsChanged(Variant sprite)
	{
		ApplyVisibility();
	}

	private void ApplyVisibility()
	{
		if (_allTargetSlots.Count == 0)
			return;

		foreach (var slot in _allTargetSlots)
		{
			string slotName = slot.Call("get_data").AsGodotObject().Call("get_name").AsString();
			ulong slotId = slot.GetInstanceId();
			string key = $"{slotId}_{slotName}"; 

			var colorVariant = slot.Call("get_color");
			Color currentColor = colorVariant.As<Color>();

			if (_isHidden)
			{
				if (!_storedColors.ContainsKey(key))
				{
					_storedColors[key] = currentColor;
				}
				currentColor.A = 0f;
				slot.Call("set_color", currentColor);
			}
			else
			{
				if (_storedColors.TryGetValue(key, out Color storedColor))
				{
					slot.Call("set_color", storedColor);
					_storedColors.Remove(key);
				}
			}
		}
	}
	
	private void _on_TextureButton_toggled(bool buttonPressed)
	{
		_isHidden = buttonPressed;
		ApplyVisibility();
	}

	private bool IsInToggleList(string name)
	{
		foreach (string target in _slotsToToggle)
		{
			if (target == name)
				return true;
		}
		return false;
	}

	public override void _ExitTree()
	{
		foreach (var spineNode in TargetSpineSprites)
		{
			if (spineNode != null && spineNode.HasSignal("world_transforms_changed"))
			{
				spineNode.Disconnect("world_transforms_changed", new Callable(this, nameof(OnWorldTransformsChanged)));
			}
		}

		_allTargetSlots.Clear();
		_storedColors.Clear();
	}
}
