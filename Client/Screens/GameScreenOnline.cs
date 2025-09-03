using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Client.Screens
{
    public class GameScreenOnline
    {
        private SpriteBatch spriteBatch;
        private Texture2D playerTexture, asteroidTexture, bulletTexture;

        public GameScreenOnline(SpriteBatch spriteBatch, Texture2D playerTexture, Texture2D asteroidTexture, Texture2D bulletTexture)
        {
            this.spriteBatch = spriteBatch;
            this.playerTexture = playerTexture;
            this.asteroidTexture = asteroidTexture;
            this.bulletTexture = bulletTexture;
        }

        // Recebe o JsonElement do servidor e desenha tudo na tela
        public void Draw(JsonElement gameState)
        {
            spriteBatch.Begin();

            // Desenha asteroides
            if (gameState.TryGetProperty("asteroids", out var asteroids))
            {
                foreach (var a in asteroids.EnumerateArray())
                {
                    float x = (float)a.GetProperty("x").GetDouble();
                    float y = (float)a.GetProperty("y").GetDouble();
                    float size = (float)a.GetProperty("size").GetDouble();
                    spriteBatch.Draw(asteroidTexture, new Vector2(x, y), null, Color.White, 0f, Vector2.Zero, size / 32f, SpriteEffects.None, 0f);
                }
            }

            // Desenha jogadores
            if (gameState.TryGetProperty("players", out var players))
            {
                foreach (var p in players.EnumerateArray())
                {
                    float x = (float)p.GetProperty("x").GetDouble();
                    float y = (float)p.GetProperty("y").GetDouble();
                    spriteBatch.Draw(playerTexture, new Vector2(x, y), Color.Cyan);
                }
            }

            // Desenha balas
            if (gameState.TryGetProperty("bullets", out var bullets))
            {
                foreach (var b in bullets.EnumerateArray())
                {
                    float x = (float)b.GetProperty("x").GetDouble();
                    float y = (float)b.GetProperty("y").GetDouble();
                    spriteBatch.Draw(bulletTexture, new Vector2(x, y), Color.Yellow);
                }
            }

            spriteBatch.End();
        }
    }
}
