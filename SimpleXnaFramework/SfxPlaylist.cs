using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EnduranceTheMaze
{
    /// <summary>
    /// Playlist to play a list of sounds in order.
    /// </summary>
    public class SfxPlaylist
    {
        #region Members
        private SoundEffectInstance sound;
        private readonly MainLoop game;

        /// <summary>
        /// The global music volume of the playlist.
        /// </summary>
        public static float musicVolume = 0.3f;

        /// <summary>
        /// The global sfx volume of the playlist.
        /// </summary>
        public static float sfxVolume = 0.2f;

        /// <summary>
        /// Sounds to be iterated through in the playlist.
        /// </summary>
        public List<SoundEffect> sounds;

        /// <summary>
        /// The current Song.
        /// </summary>
        public SoundEffectInstance Song
        {
            get
            {
                return sound;
            }
            set
            {
                if (sound != null && !sound.IsDisposed)
                {
                    sound.Dispose();
                }
                sound = value;
            }
        }

        /// <summary>
        /// The position of the Song in the list.
        /// </summary>
        public int soundIndex = 0;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new playlist for Song effects.
        /// </summary>
        /// <param name="sounds">
        /// Takes any number of sounds.
        /// </param>
        public SfxPlaylist(MainLoop game, params SoundEffect[] snds)
        {
            sounds = new List<SoundEffect>();
            this.game = game;

            foreach (SoundEffect sfx in snds)
            {
                sounds.Add(sfx);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Plays the given Song at the desired location with respect
        /// attenuated by distance to the listener position.
        /// </summary>
        /// <param name="snd">
        /// The Song to play.
        /// </param>
        /// <param name="x">
        /// The x-position of the Song, to be compared to the active
        /// actor's position.
        /// </param>
        /// <param name="y">
        /// The y-position of the Song, to be compared to the active
        /// actor's position.
        /// </param>
        public void Play(SoundEffect snd, int x, int y)
        {
            SoundEffectInstance sound = snd.CreateInstance();
            sound.Volume = sfxVolume;

            #region Interaction: MngrLvl.cs
            if (game.mngrLvl.actor != null)
            {
                //Attenuates the sfx volume based on distance to Song.
                int xPos = Math.Abs(x - game.mngrLvl.actor.X);
                int yPos = Math.Abs(y - game.mngrLvl.actor.Y);

                if (xPos + yPos != 0)
                {
                    sound.Volume = sfxVolume * (1f / (xPos + yPos));
                }
            }
            #endregion

            sound.Play();

            // disposes 50ms after playing
            Task.Run(async delegate
            {
                await Task.Delay((int)snd.Duration.TotalMilliseconds + 50);
                if (!sound.IsDisposed) { sound.Dispose(); }
            });
        }

        /// <summary>
        /// Plays the given Song.
        /// </summary>
        /// <param name="snd">
        /// The Song to play.
        /// </param>
        public static void Play(SoundEffect snd)
        {
            SoundEffectInstance sound = snd.CreateInstance();
            sound.Volume = sfxVolume;
            if (sound.Volume != 0)
            {
                sound.Play();
            }

            // disposes 50ms after playing
            Task.Run(async delegate
            {
                await Task.Delay((int)snd.Duration.TotalMilliseconds + 50);
                if (!sound.IsDisposed) { sound.Dispose(); }
            });
        }

        /// <summary>
        /// Randomly selects the next Song to play and returns Song index.
        /// Returns -1 if there are no sounds loaded.
        /// </summary>
        public int NextSongRandom()
        {
            if (sounds.Count == 0)
            {
                return -1;
            }

            soundIndex = new Random().Next(sounds.Count);

            Song = sounds.ElementAt(soundIndex).CreateInstance();
            Song.Volume = musicVolume;
            Song.Play();
            return soundIndex;
        }

        /// <summary>
        /// Pauses or resumes the active Song from the playlist.
        /// </summary>
        public void Pause(bool doPause)
        {
            if (doPause)
            {
                Song?.Pause();
            }
            else if (Song?.State == SoundState.Paused)
            {
                Song?.Resume();
            }
        }

        /// <summary>
        /// Shuffles the Song list.
        /// </summary>
        public void Shuffle()
        {
            Random rng = new Random();
            int numSongs = sounds.Count;

            while (numSongs > 1)
            {
                numSongs--;
                int next = rng.Next(numSongs + 1);
                (sounds[numSongs], sounds[next]) = (sounds[next], sounds[numSongs]);
            }
        }

        /// <summary>
        /// Checks to see if the song ended and begins the next.
        /// </summary>
        public void Update()
        {
            //Starts playing the first Song.
            if (Song == null)
            {
                NextSongRandom();
                return;
            }

            //When the Song finishes, start another.
            if (Song?.State == SoundState.Stopped)
            {
                //Keeps track of the old Song index for the loop.
                int tempSoundIndex = soundIndex;

                while (tempSoundIndex == soundIndex)
                {
                    Song.Stop();
                    NextSongRandom();
                }
            }
        }
        #endregion
    }
}
