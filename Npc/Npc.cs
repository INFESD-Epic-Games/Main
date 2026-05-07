using SpellFall.Engine;
using SpellFall.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpellFall.Items;
using SpellFall.Quests;
using SpellFall.UI;
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
        private Action onQuestAccepted;
        private HealthBar playerHealthBar;
        private KeyboardState previousKeyboard;
        private TextBubble textBubble;
        private Texture2D _indicatorTexture;
        private Vector2 _indicatorPosition;



        public void SetPlayerHealthBar(HealthBar healthBar)
        {
            playerHealthBar = healthBar;
        }

        public Npc(Point Position)
        {
            rectangleCollider = new RectangleCollider(new Rectangle(Position, Point.Zero));
            position = Position.ToVector2();
            SetCollider(rectangleCollider);
            textBubble = GameManager.GetGameManager().textBubble;
        }

        public void Initialize(QuestManager questManager, Action onQuestAccepted = null)
        {
            this.questManager = questManager;
            this.onQuestAccepted = onQuestAccepted;

            quest = new Quest(
                "KillAliens",
                "Defeat 3 aliens",
                3,
                () => Console.WriteLine("Quest completed!")
            );
        }

        public override void Load(ContentManager content)
        {
            base.Load(content);
            walkSouth = content.Load<Texture2D>("Npc Rombo");
            _indicatorTexture = content.Load<Texture2D>("NPC indicator");
            
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

            if (!hasGivenQuest || (!questCompletedRewardGiven && quest.IsCompleted))
            {
                _indicatorPosition = position + new Vector2(0, -150);

                spriteBatch.Draw(
                    _indicatorTexture,
                    _indicatorPosition,
                    null,
                    Color.White,
                    0f,
                    new Vector2(_indicatorTexture.Width / 2f, _indicatorTexture.Height / 2f),
                    4f,
                    SpriteEffects.None,
                    0f
                );
            }
            else
            {
                return;
            }
        }
        private void Interact()
        {
            // if (textBubble == null)
            // {
            //     return;
            // }

            GameState.IsPaused = true;

            if (!hasGivenQuest)
            {
                textBubble.SetText("Hello! Can you defeat 3 aliens?");
                textBubble.Show();
                questManager.AddQuest(quest);
                onQuestAccepted?.Invoke();
                onQuestAccepted = null;
                hasGivenQuest = true;
            }
            else if (!quest.IsCompleted)
            {
                textBubble.SetText("You haven't completed the quest yet...");
                textBubble.Show();
            }
            else if (!questCompletedRewardGiven)
            {
                textBubble.SetText("Good job! Here's your reward! +10 max health!");
                textBubble.Show();

                GiveReward();

                questCompletedRewardGiven = true;
            }
            else
            {
                textBubble.SetText("Thank you for your help!");
                textBubble.Show();
            }
        }

        public void ContinueDialogue()
        {
            if (textBubble != null)
            {
                textBubble.Hide();
            }

            GameState.IsPaused = false;
        }

        public void SetTextBubble(TextBubble bubble)
        {
            textBubble = bubble;
        }

        private void GiveReward()
        {
            playerHealthBar.IncreaseMaxHealth(10);
        }
    }
}
