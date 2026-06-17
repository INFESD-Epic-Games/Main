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
using SpellFall.Enemies;

namespace SpellFall.Npcs
{
    public class Npc : GameObject
    {
        private const float NpcScale = 0.5f;
        private readonly string[] introDialogueLines =
        {
            "Hello traveler!",
            "Enemies are lurking all around us.",
            "Can you clear all the rooms to help me?",
            "I would be forever grateful!",
            "Please, I need your help!"
        };

        private readonly string[] endDialogueLines =
        {
            "Thank you for clearing the rooms!",
            "I can finally rest in peace now."
        };
        private Texture2D walkSouth;
        public RectangleCollider rectangleCollider { get; private set; }
        Vector2 position;
        private Texture2D currentTexture;
        private bool playerInRange = false;
        private bool hasGivenQuest = false;
        private bool questCompletedRewardGiven = false;
        private bool dialogueActive = false;
        private bool _isFinalSpawnPlaced = false;
        private bool endDialogueActive = false;
        private bool questOver = false;
        private int dialogueStage = 0;
        private int endDialogueStage = 0;

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

        public bool IsDialogueActive => dialogueActive || endDialogueActive;

        public void Initialize(QuestManager questManager, Action onQuestAccepted = null)
        {
            this.questManager = questManager;
            this.onQuestAccepted = onQuestAccepted;

            quest = new Quest(
                "Main Quest",
                "Progress through the rooms",
                5,
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
            int scaledColliderWidth = (int)(colliderWidth * NpcScale);
            int scaledColliderHeight = (int)(colliderHeight * NpcScale);

            rectangleCollider.shape = new Rectangle(
                (position - new Vector2(scaledColliderWidth / 2f, scaledColliderHeight / 2f)).ToPoint(),
                new Point(scaledColliderWidth, scaledColliderHeight));
        }

        public override void Update(GameTime gameTime)
        {
            var player = GameManager.GetGameManager().Player;

            if (player == null)
            {
                return;
            }
            
            if (!_isFinalSpawnPlaced && GameManager.GetGameManager().GetObjectsOfType<Enemy>().Count == 0 && GameManager.GetGameManager().CurrentMap == GameManager.GetGameManager().Maps[2])
            {
                var room = GameManager.GetGameManager().CurrentMap;
                quest.IsCompleted = true;

                Vector2 targetPosition = new Vector2(room.Position.X + 1000, room.Position.Y + 1000);

                // If the player is too close to the intended spawn point, offset the NPC to avoid clipping
                float minSpawnDistance = 250f;
                if (Vector2.Distance(player.GetPosition().Center.ToVector2(), targetPosition) < minSpawnDistance)
                {
                    targetPosition.Y += 300f;
                }

                position = targetPosition;
                int w = rectangleCollider.shape.Width;
                int h = rectangleCollider.shape.Height;
                rectangleCollider.shape = new Rectangle(
                    (position - new Vector2(w / 2f, h / 2f)).ToPoint(),
                    new Point(w, h));

                _isFinalSpawnPlaced = true;
            }

            bool npcVisible = !hasGivenQuest || quest.IsCompleted;

            if (!npcVisible)
            {
                previousKeyboard = Keyboard.GetState();
                return;
            }

            Rectangle interactionBounds = rectangleCollider.shape;
            interactionBounds.Inflate(48, 48);
            playerInRange = interactionBounds.Intersects(player.GetPosition());

            KeyboardState currentKeyboard = Keyboard.GetState();

            if (playerInRange && currentKeyboard.IsKeyDown(Keys.E) && previousKeyboard.IsKeyUp(Keys.E))
            {
                Interact();
            }
            previousKeyboard = currentKeyboard;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (hasGivenQuest && !quest.IsCompleted)
            {
                return;
            }

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

            if (!hasGivenQuest || (quest.IsCompleted && !questOver))
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
        }
        private void Interact()
        {
            if (textBubble == null)
            {
                return;
            }

            GameState.IsPaused = true;

            if (!hasGivenQuest)
            {
                dialogueActive = true;
                endDialogueActive = false;
                dialogueStage = 0;
                ShowDialogueLine(dialogueStage);
            }
            else if (quest.IsCompleted && !questCompletedRewardGiven)
            {
                endDialogueActive = true;
                endDialogueStage = 0;
                ShowEndDialogueLine(endDialogueStage);
            }
            else
            {
                textBubble.SetText("Thank you for your help!");
                textBubble.Show();
            }
        }

        public void ContinueDialogue()
        {
            if (textBubble == null)
            {
                GameState.IsPaused = false;
                return;
            }

            if (dialogueActive && !hasGivenQuest)
            {
                if (dialogueStage < introDialogueLines.Length - 1)
                {
                    dialogueStage++;
                    ShowDialogueLine(dialogueStage);

                    if (dialogueStage == introDialogueLines.Length - 1)
                    {
                        questManager.AddQuest(quest);
                        onQuestAccepted?.Invoke();
                        onQuestAccepted = null;
                        hasGivenQuest = true;
                        // Move NPC off-map so it is hidden but still available later for end dialogue
                        position = new Vector2(-10000f, -10000f);
                        int w = rectangleCollider.shape.Width;
                        int h = rectangleCollider.shape.Height;
                        rectangleCollider.shape = new Rectangle(
                            (position - new Vector2(w / 2f, h / 2f)).ToPoint(),
                            new Point(w, h));
                    }

                    return;
                }

                dialogueActive = false;
                dialogueStage = 0;
                textBubble.Hide();
                GameState.IsPaused = false;
                return;
            }

            if (endDialogueActive)
            {
                if (endDialogueStage < endDialogueLines.Length - 1)
                {
                    endDialogueStage++;
                    ShowEndDialogueLine(endDialogueStage);

                    if (endDialogueStage == endDialogueLines.Length - 1)
                    {
                        GiveReward();
                        questCompletedRewardGiven = true;
                    }

                    return;
                }

                endDialogueActive = false;
                endDialogueStage = 0;
                textBubble.Hide();
                GameState.IsPaused = false;
                questOver = true;
                return;
            }

            textBubble.Hide();
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

        private void ShowDialogueLine(int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= introDialogueLines.Length)
            {
                return;
            }

            textBubble.SetText(introDialogueLines[lineIndex]);
            textBubble.Show();
        }

        private void ShowEndDialogueLine(int lineIndex)
        {
            if (lineIndex < 0 || lineIndex >= endDialogueLines.Length)
            {
                return;
            }

            textBubble.SetText(endDialogueLines[lineIndex]);
            textBubble.Show();
        }
    }
}
