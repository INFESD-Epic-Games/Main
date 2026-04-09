using SpellFall.Engine;
using SpellFall.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Items;
using SpellFall.Quests;
using Microsoft.Xna.Framework.Input;
using System;
using SpellFall.Character;

namespace SpellFall.Npcs
{
    public class Npc : GameObject
    {
        private const float NpcScale = 0.5f;
        private Texture2D walkSouth;
        public RectangleCollider rectangleCollider { get; private set; }
        Vector2 position;
        private Texture2D currentTexture;
        private bool playerInRange = false;
        private bool hasGivenQuest = false;
        private bool questCompletedRewardGiven = false;

        private Quest quest;
        private QuestManager questManager;
        private HealthBar playerHealthBar;
        private KeyboardState previousKeyboard;



        public void SetPlayerHealthBar(HealthBar healthBar)
        {
            playerHealthBar = healthBar;
        }

        public Npc(Point Position)
        {
            rectangleCollider = new RectangleCollider(new Rectangle(Position, Point.Zero));
            position = Position.ToVector2();
            SetCollider(rectangleCollider);
        }

        public void Initialize(QuestManager questManager)
        {
            this.questManager = questManager;

            quest = new Quest(
                "KillAliens",
                "Versla 3 aliens",
                3,
                () => Console.WriteLine("Quest completed!")
            );
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            walkSouth = content.Load<Texture2D>("Npc Rombo");
            
            currentTexture = walkSouth;

            int colliderWidth = currentTexture.Width;
            int colliderHeight = currentTexture.Height / 4;
            rectangleCollider.shape.Size = new Point(colliderWidth, colliderHeight);
            rectangleCollider.shape.Location -= new Point(colliderWidth / 2, colliderHeight / 2);
        }

        public override void Update(GameTime gameTime)
        {
            var player = GameManager.GetGameManager().Player;

            float distance = Vector2.DistanceSquared(position, player.GetPosition().Center.ToVector2());

            playerInRange = distance < 10000f;

            KeyboardState currentKeyboard = Keyboard.GetState();

            if (playerInRange && currentKeyboard.IsKeyDown(Keys.E) && previousKeyboard.IsKeyUp(Keys.E))
                {
                    Interact();
                }
            previousKeyboard = currentKeyboard;
        }
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            int frameHeight = currentTexture.Height / 4;
            int frameWidth = currentTexture.Width;

            int idleFrame = 1;

            Rectangle sourceRect = new Rectangle(
                0,
                frameHeight * idleFrame,
                frameWidth,
                frameHeight
            );

            spriteBatch.Draw(
                currentTexture,
                position,
                sourceRect,
                Color.White,
                0f,
                new Vector2(frameWidth / 2f, frameHeight / 2f),
                NpcScale,
                SpriteEffects.None,
                0f
            );
        }
        private void Interact()
        {
            if (!hasGivenQuest)
            {
                Console.WriteLine("NPC: Hallo! Kun je 3 aliens verslaan?");
                questManager.AddQuest(quest);
                System.Console.WriteLine(questManager.ActiveQuests);
                hasGivenQuest = true;
            }
            else if (!quest.IsCompleted)
            {
                System.Console.WriteLine(quest.Name);
                Console.WriteLine("NPC: Je bent nog niet klaar...");
            }
            else if (!questCompletedRewardGiven)
            {
                Console.WriteLine("NPC: Goed gedaan! Hier is je beloning!");

                GiveReward();

                questCompletedRewardGiven = true;
            }
            else
            {
                Console.WriteLine("NPC: Bedankt voor je hulp!");
            }
        }

        private void GiveReward()
        {
            Console.WriteLine("Player krijgt +10 maxhealth!");

            // voorbeeld:
            playerHealthBar.IncreaseMaxHealth(10);
        }
    }
}
