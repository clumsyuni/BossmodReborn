namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D031Lozatl;

public enum OID : uint
{
    Boss = 0x28E7, // R=4.4
    GravenGatekeep = 0x28E8,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Stonefist = 15497, // Boss->player, 4.0s cast, single-target
    SunToss = 15498, // Boss->location, 3.0s cast, range 5 circle
    LozatlsScorn = 15499, // Boss->self, 3.0s cast, range 40 circle
    RonkanLightRight = 15500, // Helper->self, no cast, range 60 width 20 rect
    RonkanLightLeft = 15725, // Helper->self, no cast, range 60 width 20 rect
    HeatUp = 15502, // Boss->self, 3.0s cast, single-target
    HeatUp2 = 15501, // Boss->self, 3.0s cast, single-target
    LozatlsFury1 = 15504, // Boss->self, 4.0s cast, range 60 width 20 rect
    LozatlsFury2 = 15503 // Boss->self, 4.0s cast, range 60 width 20 rect
}

sealed class LozatlsFury(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LozatlsFury1, (uint)AID.LozatlsFury2], new AOEShapeRect(60f, 10f));

class Stonefist(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Stonefist);
class LozatlsScorn(BossModule module) : Components.RaidwideCast(module, (uint)AID.LozatlsScorn);
class SunToss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SunToss, 5f);

class RonkanLight(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(60f, 20f);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_aoe != null)
            yield return _aoe.Value;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        // 0x19D 
        if (state == 0x19Du && (OID)actor.OID == OID.GravenGatekeep)
        {
            // Determine side based on statue X position relative to arena center
            // Statues are roughly at X = -17.6 and X = +17.6
            bool isLeft = actor.Position.X < module.Center.X;
            
            // 90 degrees points Left (West), -90 degrees points Right (East)
            Angle rot = isLeft ? 90f.Degrees() : -90f.Degrees();

            _aoe = new AOEInstance(rect, module.Center, rot, WorldState.FutureTime(8.2d));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RonkanLightLeft or (uint)AID.RonkanLightRight)
        {
            _aoe = null;
        }
    }
}

class D031LozatlStates : StateMachineBuilder
{
    public D031LozatlStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LozatlsFury>()
            .ActivateOnEnter<Stonefist>()
            .ActivateOnEnter<SunToss>()
            .ActivateOnEnter<RonkanLight>()
            .ActivateOnEnter<LozatlsScorn>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651, NameID = 8231)]
public class D031Lozatl(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(0f, 315f);
    private static readonly ArenaBoundsCustom arena = new([new Polygon(ArenaCenter, 19.5f * CosPI.Pi40th, 40)],
    [new Rectangle(new(0f, 335.1f), 20f, 2f), new Rectangle(new(0f, 294.5f), 20f, 2f)]);

    protected override void Update(int slot, Actor actor)
    {
        // Optional: The boss moves to Y=425 later. 
        // If the arena bounds look wrong during the second half, 
        // we can update Center here.
    }
}
