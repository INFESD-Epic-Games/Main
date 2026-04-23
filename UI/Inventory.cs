using System.Collections.Generic;
using System.ComponentModel;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Forms.Controls;
using Gum.Wireframe;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using RenderingLibrary.Graphics;
using SpellFall.Character;
using SpellFall.Items;

namespace SpellFall.UI;

public class Inventory
{
    public List<Item> Items => _player.Inventory;
    
    private readonly Player _player;
    
    private readonly Panel _panel;
    private NineSliceRuntime _background;

    private ContainerRuntime _itemsContainer;
    private Panel _itemsPanel;
    private NineSliceRuntime _itemsBackground;
    
    private ContainerRuntime _characterContainer;
    private Panel _characterPanel;
    private NineSliceRuntime _characterBackground;
    
    private ContainerRuntime _uniqueContainer;
    private Panel _uniquePanel;
    private NineSliceRuntime _uniqueBackground;

    public Inventory(Player player, ContentManager content, int margin = 96)
    {
        _player = player;
        
        Texture2D backgroundTexture = content.Load<Texture2D>("brown");
        Texture2D inlayTexture = content.Load<Texture2D>("brown_pressed");

        _panel = new Panel
        {
            XUnits = GeneralUnitType.PixelsFromMiddle,
            YUnits = GeneralUnitType.PixelsFromMiddle,
            XOrigin = HorizontalAlignment.Center,
            YOrigin = VerticalAlignment.Center,
            WidthUnits = DimensionUnitType.RelativeToParent,
            HeightUnits = DimensionUnitType.RelativeToParent,
            
            Width = -margin * 2,
            Height = -margin * 2,
            
            IsVisible = false,
        };
        
        _panel.AddToRoot();
        
        InitBackground(backgroundTexture);
        InitItems(inlayTexture);
        InitCharacter(inlayTexture);
        InitUnique(inlayTexture);
    }

    private void InitBackground(Texture2D texture)
    {
        _background = new NineSliceRuntime
        {
            IsEnabled = true,
            HasEvents = true,
            
            Texture = texture,
            TextureWidth = 48,
            TextureHeight = 48,
            TextureLeft = 0,
            TextureTop = 0,
        };
        
        _background.Dock(Dock.Fill);
        
        _panel.AddChild(_background);
    }

    private void InitItems(Texture2D texture)
    {
        _itemsContainer = new ContainerRuntime
        {
            WidthUnits = DimensionUnitType.PercentageOfParent,
            HeightUnits = DimensionUnitType.RelativeToParent,
            
            Width = 70,
            Height = 0,
        };
        
        _panel.AddChild(_itemsContainer);
        
        _itemsPanel = new Panel
        {
            XUnits = GeneralUnitType.PixelsFromSmall,
            YUnits = GeneralUnitType.PixelsFromLarge,
            XOrigin = HorizontalAlignment.Left,
            YOrigin = VerticalAlignment.Bottom,
            WidthUnits = DimensionUnitType.RelativeToParent,
            HeightUnits = DimensionUnitType.RelativeToParent,
            
            Width = -8 -16,
            Height = -64,
            
            X = 16,
            Y = -16,
        };
        
        _itemsContainer.AddChild(_itemsPanel);
        
        _itemsBackground = new NineSliceRuntime
        {
            IsEnabled = true,
            HasEvents = true,
            
            Texture = texture,
            TextureWidth = 48,
            TextureHeight = 48,
            TextureLeft = 0,
            TextureTop = 0,
        };
        
        _itemsBackground.Dock(Dock.Fill);
        
        _itemsPanel.AddChild(_itemsBackground);
    }
    
    private void InitCharacter(Texture2D texture)
    {
        _characterContainer = new ContainerRuntime
        {
            WidthUnits = DimensionUnitType.PercentageOfParent,
            HeightUnits = DimensionUnitType.PercentageOfParent,
            XUnits = GeneralUnitType.PixelsFromLarge,
            YUnits = GeneralUnitType.PixelsFromSmall,
            XOrigin = HorizontalAlignment.Right,
            YOrigin = VerticalAlignment.Top,
            
            Width = 30,
            Height = 50,
        };
        
        _panel.AddChild(_characterContainer);
        
        _characterPanel = new Panel
        {
            XUnits = GeneralUnitType.PixelsFromLarge,
            YUnits = GeneralUnitType.PixelsFromSmall,
            XOrigin = HorizontalAlignment.Right,
            YOrigin = VerticalAlignment.Top,
            WidthUnits = DimensionUnitType.RelativeToParent,
            HeightUnits = DimensionUnitType.RelativeToParent,
            
            Width = -8 -16,
            Height = -64,
            
            X = -16,
            Y = 48,
        };
        
        _characterContainer.AddChild(_characterPanel);
        
        _characterBackground = new NineSliceRuntime
        {
            IsEnabled = true,
            HasEvents = true,
            
            Texture = texture,
            TextureWidth = 48,
            TextureHeight = 48,
            TextureLeft = 0,
            TextureTop = 0,
        };
        
        _characterBackground.Dock(Dock.Fill);
        
        _characterPanel.AddChild(_characterBackground);
    }
    
    private void InitUnique(Texture2D texture)
    {
        _uniqueContainer = new ContainerRuntime
        {
            WidthUnits = DimensionUnitType.PercentageOfParent,
            HeightUnits = DimensionUnitType.PercentageOfParent,
            XUnits = GeneralUnitType.PixelsFromLarge,
            YUnits = GeneralUnitType.PixelsFromLarge,
            XOrigin = HorizontalAlignment.Right,
            YOrigin = VerticalAlignment.Bottom,
            
            Width = 30,
            Height = 50,
        };
        
        _panel.AddChild(_uniqueContainer);
        
        _uniquePanel = new Panel
        {
            XUnits = GeneralUnitType.PixelsFromLarge,
            YUnits = GeneralUnitType.PixelsFromLarge,
            XOrigin = HorizontalAlignment.Right,
            YOrigin = VerticalAlignment.Bottom,
            WidthUnits = DimensionUnitType.RelativeToParent,
            HeightUnits = DimensionUnitType.RelativeToParent,
            
            Width = -8 -16,
            Height = -8 -16,
            
            X = -16,
            Y = -16,
        };
        
        _uniqueContainer.AddChild(_uniquePanel);
        
        _uniqueBackground = new NineSliceRuntime
        {
            IsEnabled = true,
            HasEvents = true,
            
            Texture = texture,
            TextureWidth = 48,
            TextureHeight = 48,
            TextureLeft = 0,
            TextureTop = 0,
        };
        
        _uniqueBackground.Dock(Dock.Fill);
        
        _uniquePanel.AddChild(_uniqueBackground);
    }

    public void Show(bool visible = true)
    {
        _panel.IsVisible = visible;
    }
}