using System;
using System.Collections.Generic;

namespace Primal.Gimmicks
{
    /// <summary>
    /// Shared mechanism #2 from COMMUNITY_CHALLENGES.md: "N objects/creatures present in this
    /// shelter at hibernation, deduplicated by some key". Covers the rating-3 end of Catalogue
    /// (15 unique objects/creatures), Scholar (unique pearl colors), Mother (7 Noodlefly Eggs), and
    /// the "creature present at hibernation" half of Charged/Sleepover/Symbiote.
    ///
    /// Query-only -- this file has no state and patches nothing itself. Every future per-challenge
    /// tracker is expected to subscribe to SurvivorEvents.OnSuccessfulHibernation on its own (that
    /// event already fires at the one correct moment -- see its own doc comment for why it has to
    /// be a Prefix on Win, not a Postfix) and call EntitiesInShelter/CountDistinct from inside that
    /// callback, the same way SurvivorRegionMarkers does for OnFallDamageDeath.
    /// </summary>
    internal static class SurvivorShelterContents
    {
        /// <summary>The player's current room, but only if it's actually a shelter (AbstractRoom
        /// .shelter, confirmed via decompilation to be `shelterIndex &gt; -1` -- a real vanilla
        /// distinction between a shelter and an ordinary room, not something this mod has to
        /// detect itself via ShelterDoor presence or similar). Null off a shelter, or if the
        /// player's abstract room can't be resolved at all. Every one of these challenges only
        /// cares about the moment of a successful hibernation, so callers are expected to already
        /// be inside a SurvivorEvents.OnSuccessfulHibernation handler when they call this -- at
        /// that exact moment the player is, by construction, standing in the shelter that just
        /// triggered Win, so this should never come back null in practice there.</summary>
        public static AbstractRoom ShelterRoomFor(Player player)
        {
            AbstractRoom room = player?.abstractCreature?.Room;
            return room != null && room.shelter ? room : null;
        }

        /// <summary>Every AbstractWorldEntity (creature or item) currently recorded in this
        /// shelter's abstract room -- AbstractRoom.entities, confirmed via decompilation to be the
        /// same list both AbstractCreature and AbstractPhysicalObject register themselves into via
        /// their shared AbstractWorldEntity.Room property. Excludes the player themselves (every
        /// one of these challenges counts what the player BROUGHT or PLACED, not the player's own
        /// body) and anything already flagged for removal (slatedForDeletion), which would
        /// otherwise still show up for one more frame after being consumed/destroyed.</summary>
        public static IEnumerable<AbstractWorldEntity> EntitiesInShelter(Player player)
        {
            AbstractRoom room = ShelterRoomFor(player);
            if (room?.entities == null)
            {
                yield break;
            }

            AbstractCreature playerAbstract = player.abstractCreature;
            foreach (AbstractWorldEntity entity in room.entities)
            {
                if (entity == null || entity == playerAbstract || entity.slatedForDeletion)
                {
                    continue;
                }
                yield return entity;
            }
        }

        /// <summary>How many distinct keys (as produced by `keySelector`, e.g. an icon name, a
        /// pearl color, a CreatureTemplate.Type) appear among the shelter's entities, after an
        /// optional filter (e.g. "only AbstractPhysicalObject", "only this CreatureTemplate.Type").
        /// This is the one piece of real logic in this file -- everything else is just resolving
        /// which room and which entities to look at.</summary>
        public static int CountDistinct<TKey>(
            Player player,
            Func<AbstractWorldEntity, TKey> keySelector,
            Func<AbstractWorldEntity, bool> filter = null)
        {
            var seen = new HashSet<TKey>();
            foreach (AbstractWorldEntity entity in EntitiesInShelter(player))
            {
                if (filter != null && !filter(entity))
                {
                    continue;
                }
                TKey key = keySelector(entity);
                if (key != null)
                {
                    seen.Add(key);
                }
            }
            return seen.Count;
        }

        /// <summary>True if a live (non-dead) creature of this exact CreatureTemplate.Type is
        /// present in the shelter -- the shape Charged/Sleepover/Symbiote/Mother all need ("is my
        /// specific carried creature here with me"), parameterized on type rather than hardcoded to
        /// any one species since several challenges share this exact check against different
        /// species.</summary>
        public static bool HasLiveCreatureOfType(Player player, CreatureTemplate.Type type)
        {
            foreach (AbstractWorldEntity entity in EntitiesInShelter(player))
            {
                if (entity is AbstractCreature creature
                    && creature.creatureTemplate?.type == type
                    && !creature.state.dead)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>How many INSTANCES of this exact AbstractObjectType are present in the
        /// shelter -- Mother's "7 Noodlefly Eggs" needs a raw instance count of one item type, not
        /// a distinct-type count the way CountDistinct answers, since all 7 eggs share the same
        /// type. Items only (AbstractPhysicalObject), not creatures -- eggs, pearls, spears etc.
        /// are all inanimate AbstractPhysicalObjects with their own `type` field (a different enum,
        /// AbstractObjectType, from creatures' own CreatureTemplate.Type).</summary>
        public static int CountObjectsOfType(Player player, AbstractPhysicalObject.AbstractObjectType type)
        {
            int count = 0;
            foreach (AbstractWorldEntity entity in EntitiesInShelter(player))
            {
                if (entity is AbstractPhysicalObject obj && !(obj is AbstractCreature) && obj.type == type)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
