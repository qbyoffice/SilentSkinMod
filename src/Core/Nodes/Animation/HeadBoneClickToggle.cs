using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace SilentSkinMod.Core.Nodes.Animation;

[GlobalClass]
[GodotClassName("HeadBoneClickToggle")]
public partial class HeadBoneClickToggle : Control
{
    private readonly string[] _slotsToToggle = new string[]
    {
        "tougu mianju 1",
        "tougu mianju 0",
        "mianju yanjing 0",
        "mianju yanjing 1",
        "tougu mianju 2",
        "mianju yinying 0"
    };

    private Node _spineNode = null;

    public override void _Ready()
    {
        _spineNode = GetNode("../SpineSprite");
        if (_spineNode == null)
        {
            GD.PrintErr("未找到 SpineSprite 节点，请检查路径 '../SpineSprite'");
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
        {
            if (GetGlobalRect().HasPoint(GetGlobalMousePosition()))
            {
                ToggleSlotsVisibility();
            }
        }
    }

    private void ToggleSlotsVisibility()
    {
        if (_spineNode == null)
            return;
        
        var spineSprite = new MegaSprite(Variant.From(_spineNode));
        var skeleton = spineSprite.GetSkeleton();
        if (skeleton == null)
        {
            GD.PrintErr("无法获取骨骼");
            return;
        }

        foreach (string slotName in _slotsToToggle)
        {
            var slot = skeleton.BoundObject.Call("find_slot", slotName);
            if (slot.VariantType == Variant.Type.Nil)
            {
                GD.PrintErr($"插槽 '{slotName}' 不存在");
                continue;
            }
            
            var colorVariant = slot.AsGodotObject().Call("get_color");
            Color color = colorVariant.As<Color>();
            
            color.A = color.A > 0.5f ? 0f : 1f;
            
            slot.AsGodotObject().Call("set_color", color);
        }
    }
}