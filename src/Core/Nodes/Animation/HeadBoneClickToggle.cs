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
        "mianju yinying 0"
    };

    private bool _isHidden = false; 
    private Node _spineNode = null; 
    private MegaSkeleton _skeleton = null; 
    private List<GodotObject> _targetSlots = new List<GodotObject>(); 
    private Dictionary<string, Color> _storedColors = new Dictionary<string, Color>(); 

    public override void _Ready()
    {
        _spineNode = GetNode("../SpineSprite");
        
        var spineSprite = new MegaSprite(Variant.From(_spineNode));
        _skeleton = spineSprite.GetSkeleton();
        
        if (_spineNode.HasSignal("world_transforms_changed"))
        {
            _spineNode.Connect("world_transforms_changed", new Callable(this, nameof(OnWorldTransformsChanged)));
        }
        
        CacheTargetSlots();
    }
    
    private void CacheTargetSlots()
    {
        var slots = _skeleton.BoundObject.Call("get_slots");
        if (slots.VariantType != Variant.Type.Array)
            return;

        var slotsArray = slots.As<Godot.Collections.Array>();
        foreach (var slotVariant in slotsArray)
        {
            if (slotVariant.VariantType != Variant.Type.Object)
                continue;

            var slot = slotVariant.AsGodotObject();
            string slotName = slot.Call("get_data").AsGodotObject().Call("get_name").AsString();

            if (IsInToggleList(slotName))
            {
                _targetSlots.Add(slot);
            }
        }
    }
    
    private void OnWorldTransformsChanged(Variant sprite)
    {
        ApplyVisibility();
    }
    
    private void ApplyVisibility()
    {
        if (_skeleton == null || _targetSlots.Count == 0)
            return;

        foreach (var slot in _targetSlots)
        {
            string slotName = slot.Call("get_data").AsGodotObject().Call("get_name").AsString();
            
            var colorVariant = slot.Call("get_color");
            Color currentColor = colorVariant.As<Color>();

            if (_isHidden)
            {
                if (!_storedColors.ContainsKey(slotName))
                {
                    _storedColors[slotName] = currentColor;
                }
                currentColor.A = 0f;
                slot.Call("set_color", currentColor);
            }
            else
            {
                if (_storedColors.TryGetValue(slotName, out Color storedColor))
                {
                    slot.Call("set_color", storedColor);
                    _storedColors.Remove(slotName);
                }
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent &&
            mouseEvent.ButtonIndex == MouseButton.Left &&
            mouseEvent.Pressed)
        {
            if (GetGlobalRect().HasPoint(GetGlobalMousePosition()))
            {
                _isHidden = !_isHidden;
                ApplyVisibility(); 
            }
        }
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
        if (_spineNode != null && _spineNode.HasSignal("world_transforms_changed"))
        {
            _spineNode.Disconnect("world_transforms_changed", new Callable(this, nameof(OnWorldTransformsChanged)));
        }
        
        _targetSlots.Clear();
        _storedColors.Clear();
    }
}