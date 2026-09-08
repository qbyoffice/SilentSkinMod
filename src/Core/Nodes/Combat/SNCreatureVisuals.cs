using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Entities.Creatures;
using System.Reflection;

namespace SilentSkinMod.Core.Nodes.Combat;

[GlobalClass]
public partial class SNCreatureVisuals : NCreatureVisuals
{
    private Node2D _nekoVisuals;
    private MegaSkeletonDataResource _originalSkeletonData;
    private MegaSkeletonDataResource _nekoSkeletonData;
    private bool _isNekoActive = false;

    public override void _Ready()
    {
        base._Ready();
        
        if (SpineBody != null)
        {
            var skeleton = SpineBody.GetSkeleton();
            if (skeleton != null)
                _originalSkeletonData = skeleton.GetData();
        }

        _nekoVisuals = GetNodeOrNull<Node2D>("%Visuals_neko");
        if (_nekoVisuals == null)
        {
            GD.PrintErr("未找到 %Visuals_neko");
            return;
        }
        _nekoVisuals.Visible = false;
        
        if (_nekoVisuals.GetClass() == "SpineSprite")
        {
            var nekoSpine = new MegaSprite(_nekoVisuals);
            var skeleton = nekoSpine.GetSkeleton();
            if (skeleton != null)
                _nekoSkeletonData = skeleton.GetData();
        }

        HeadVisibilityBus.OnVisibilityChanged += OnVisibilityChanged;
        ApplyModelVisibility(HeadVisibilityBus.IsHidden);
    }

    private void OnVisibilityChanged(bool hidden) => ApplyModelVisibility(hidden);

    private void ApplyModelVisibility(bool hidden)
    {
        if (_nekoVisuals == null || SpineBody == null) return;

        if (hidden && !_isNekoActive)
        {
            if (_nekoSkeletonData != null)
            {
                SpineBody.SetSkeletonDataRes(_nekoSkeletonData);
                var skeleton = SpineBody.GetSkeleton();
                skeleton?.SetSlotsToSetupPose();
            }
            _nekoVisuals.Visible = false;
            _isNekoActive = true;
            RefreshAnimator();
            GD.Print("[KaguyaSilentNCreatureVisuals] Switched to Neko data");
        }
        else if (!hidden && _isNekoActive)
        {
            if (_originalSkeletonData != null)
            {
                SpineBody.SetSkeletonDataRes(_originalSkeletonData);
                var skeleton = SpineBody.GetSkeleton();
                skeleton?.SetSlotsToSetupPose();
            }
            _isNekoActive = false;
            RefreshAnimator();
            GD.Print("[KaguyaSilentNCreatureVisuals] Switched to Original data");
        }
    }

    private void RefreshAnimator()
    {
        var parent = GetParent();
        if (parent == null) return;

        var entityProp = parent.GetType().GetProperty("Entity");
        if (entityProp == null) return;
        var creature = entityProp.GetValue(parent) as Creature;
        if (creature == null) return;

        object model = null;
        if (creature.Player != null)
            model = creature.Player.Character;
        else if (creature.Monster != null)
            model = creature.Monster;

        if (model == null) return;

        var genMethod = model.GetType().GetMethod("GenerateAnimator", new[] { typeof(MegaSprite), typeof(Creature) });
        if (genMethod == null) return;
        var newAnimator = genMethod.Invoke(model, new object[] { SpineBody, creature }) as CreatureAnimator;
        if (newAnimator == null) return;

        var animField = parent.GetType().GetField("_spineAnimator", BindingFlags.NonPublic | BindingFlags.Instance);
        if (animField == null) return;
        animField.SetValue(parent, newAnimator);

        var connectMethod = parent.GetType().GetMethod("ConnectSpineAnimatorSignals", BindingFlags.NonPublic | BindingFlags.Instance);
        connectMethod?.Invoke(parent, null);

        var setTrigger = parent.GetType().GetMethod("SetAnimationTrigger");
        setTrigger?.Invoke(parent, new object[] { "Idle" });
    }

    public override void _ExitTree()
    {
        HeadVisibilityBus.OnVisibilityChanged -= OnVisibilityChanged;
        base._ExitTree();
    }
}