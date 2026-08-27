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
        "mianju yanjing 2", 
        "mianju yanjing 6",
        "mianju yanjing 1",
        "mianju yanjing 3",
        "mianju yanjing 4",
        "mianju yanjing 5",
        "mianju yinying 0"
    };

    private Node _spineNode = null;

    public override void _Ready()
    {
        _spineNode = GetNode("../SpineSprite");
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
            return;
        }

        foreach (string slotName in _slotsToToggle)
        {
            var slot = skeleton.BoundObject.Call("find_slot", slotName);
            if (slot.VariantType == Variant.Type.Nil)
            {
                continue;
            }
            
            var colorVariant = slot.AsGodotObject().Call("get_color");
            Color color = colorVariant.As<Color>();
            
            color.A = color.A > 0.5f ? 0f : 1f;
            
            slot.AsGodotObject().Call("set_color", color);
        }
    }
}