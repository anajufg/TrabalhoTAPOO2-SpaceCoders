using Microsoft.Xna.Framework;
using Client.Entities;
using Monogame.Processing;
using System;
using System.Collections.Generic;

public class AsteroidFactory
{
    private readonly GameAsteroids game;
    private readonly Random rnd = new Random();

    public AsteroidFactory(GameAsteroids game)
    {
        this.game = game;
    }

    public Asteroid CreateAsteroid(bool isGame)
    {
        float x = 0, y = 0, speed = 0, size = 0;
        Vector2 dir = Vector2.Zero;
        PImage asteroidSprite;

        if (isGame)
        {
            int edge = rnd.Next(4);
            switch (edge)
            {
                case 0: // top
                    x = rnd.Next(game.width);
                    y = -30;
                    dir = new Vector2((float)(rnd.NextDouble() - 0.5), 1f);
                    break;
                case 1: // bottom
                    x = rnd.Next(game.width);
                    y = game.height + 30;
                    dir = new Vector2((float)(rnd.NextDouble() - 0.5), -1f);
                    break;
                case 2: // left
                    x = -30;
                    y = rnd.Next(game.height);
                    dir = new Vector2(1f, (float)(rnd.NextDouble() - 0.5));
                    break;
                case 3: // right
                    x = game.width + 30;
                    y = rnd.Next(game.height);
                    dir = new Vector2(-1f, (float)(rnd.NextDouble() - 0.5));
                    break;
            }

            speed = 2f + (float)rnd.NextDouble() * 2f;
            size = 30f + (float)rnd.NextDouble() * 70f;
            asteroidSprite = game.gameAsteroidsSprites[rnd.Next(game.gameAsteroidsSprites.Count)];
        }
        else
        {
            x = game.width / 2 + rnd.Next(50, 400);
            y = -30 - rnd.Next(0, 50);

            float targetX = -350;
            float targetY = game.height + 30;
            dir = new Vector2(targetX - x, targetY - y);

            speed = 0.8f + (float)rnd.NextDouble() * 0.5f;
            size = 15f + (float)rnd.NextDouble() * 50f;

            asteroidSprite = size < 30f ? game.asteroidSpriteSmall : game.screenAsteroidsSprites[rnd.Next(game.screenAsteroidsSprites.Count)];
        }

        dir.Normalize();
        return new Asteroid(new Vector2(x, y), dir * speed, size, asteroidSprite, game, isGame);
    }
}
