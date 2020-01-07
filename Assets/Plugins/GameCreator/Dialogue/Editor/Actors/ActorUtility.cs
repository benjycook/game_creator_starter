namespace GameCreator.Dialogue
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class ActorUtility
    {
        private const int MAX_CACHE = 5;
        public static List<Actor> ACTORS = new List<Actor>();

        public static void Add(Actor actor)
        {
            if (actor == null) return;

            ACTORS.Remove(actor);
            ACTORS.Insert(0, actor);

            while (ACTORS.Count > MAX_CACHE)
            {
                ACTORS.RemoveAt(ACTORS.Count - 1);
            }
        }
    }
}