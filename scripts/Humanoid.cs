using Godot;
using System;
using System.Text.Json;
using System.Collections.Generic;

public partial class Humanoid : Entity
{
    // private Dictionary<string, Texture2D[]> clothesOptions = new Dictionary<string, Texture2D[]>
    // {
    //     { "head", new[] { (Texture2D)GD.Load("res://resources/entities/head.png"), (Texture2D)GD.Load("res://resources/gear/light/vine/head.png") } },
    //     { "torso", new[] { (Texture2D)GD.Load("res://resources/entities/torso.png"), (Texture2D)GD.Load("res://resources/gear/light/vine/torso.png") } },
    //     { "shoulder-right", new[] { (Texture2D)GD.Load("res://resources/entities/shoulder-right.png"), (Texture2D)GD.Load("res://resources/gear/light/vine/shoulder-right.png") } },
    //     { "hand-right", new[] { (Texture2D)GD.Load("res://resources/entities/hand-right.png"), (Texture2D)GD.Load("res://resources/gear/light/vine/hand-right.png") } },
    //     { "shoulder-left", new[] { (Texture2D)GD.Load("res://resources/entities/shoulder-left.png"), (Texture2D)GD.Load("res://resources/gear/light/vine/shoulder-left.png") } },
    //     { "hand-left", new[] { (Texture2D)GD.Load("res://resources/entities/hand-left.png"), (Texture2D)GD.Load("res://resources/gear/light/vine/hand-left.png") } },
    //     { "leg-right", new[] { (Texture2D)GD.Load("res://resources/entities/leg-right.png"), (Texture2D)GD.Load("res://resources/gear/light/vine/leg-right.png") } },
    //     { "foot-right", new[] { (Texture2D)GD.Load("res://resources/entities/foot-right.png"), (Texture2D)GD.Load("res://resources/gear/light/vine/foot-right.png") } },
    //     { "leg-left", new[] { (Texture2D)GD.Load("res://resources/entities/leg-left.png"), (Texture2D)GD.Load("res://resources/gear/light/vine/leg-left.png") } },
    //     { "foot-left", new[] { (Texture2D)GD.Load("res://resources/entities/foot-left.png"), (Texture2D)GD.Load("res://resources/gear/light/vine/foot-left.png") } }
    // };

    public override void _Ready()
    {
        // var button = new Button() { Text = "Change Clothes" };
        // button.Pressed += () =>
        // {
        //     ChangeClothes("head", 1);
        //     ChangeClothes("torso", 1);
        //     ChangeClothes("shoulder-right", 1);
        //     ChangeClothes("hand-right", 1);
        //     ChangeClothes("shoulder-left", 1);
        //     ChangeClothes("hand-left", 1);
        //     ChangeClothes("leg-right", 1);
        //     ChangeClothes("foot-right", 1);
        //     ChangeClothes("leg-left", 1);
        //     ChangeClothes("foot-left", 1);
        // };
        // AddChild(button);

        // LoadAnimation(currentAnimationPath);
        // StoreAnimation();

    }

    public void SetGear(GearSlot slot, string? name)
    {
        switch (slot)
        {
            case GearSlot.Head:
                ChangeClothes("head", name);
                break;
            case GearSlot.Torso:
                ChangeClothes("torso", name);
                break;
            case GearSlot.Arms:
                ChangeClothes("shoulder-right", name);
                ChangeClothes("hand-right", name);
                ChangeClothes("shoulder-left", name);
                ChangeClothes("hand-left", name);
                break;
            case GearSlot.Legs:
                ChangeClothes("leg-right", name);
                ChangeClothes("foot-right", name);
                ChangeClothes("leg-left", name);
                ChangeClothes("foot-left", name);
                break;
            case GearSlot.Weapon:
                ChangeClothes("weapon", name);
                if (name.StartsWith("bow"))
                    ChangeClothes("weapon-extra", "images/bow/bow-string");
                break;
        }
    }

    private string currentAnimationPath = "res://resources/entities/player/run.json";

    private Dictionary<string, Dictionary<string, BoneTransform>> _frames;
    private float _timer = 0f;
    private int _currentFrame = 1;
    private int _totalFrames = 0;
    private class BoneTransform
    {
        public float x { get; set; }
        public float y { get; set; }
        public float rotation { get; set; }
    }

    private void LoadAnimation(string path)
    {
        var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr("Failed to open animation file: " + path);
            return;
        }

        string json = file.GetAsText();
        file.Close();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var raw = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, BoneTransform>>>(json, options);

        _frames = raw;
        _totalFrames = raw.Count;
    }
    [Export] public float FramesPerSecond = 30.0f;
    public override void _Process(double delta)
    {
        // if (_frames == null || _totalFrames == 0) return;

        // _timer += (float)delta;
        // float frameDuration = 1.0f / FramesPerSecond;

        // if (_timer >= frameDuration)
        // {
        //     _timer -= frameDuration;
        //     _currentFrame++;
        //     if (_currentFrame > _totalFrames)
        //         _currentFrame = 1;

        //     var frameData = LoadFrameData(_currentFrame.ToString());
        //     ApplyFrame(frameData);
        // }
    }

    [Export] public AnimationPlayer AnimationPlayer;
    [Export] public string AnimationName = "run";
    [Export] public float FrameDuration = 0.1f;

    public void StoreAnimation()
    {
        string json = FileAccess.GetFileAsString(currentAnimationPath);
        var animationData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, TransformFrame>>>(json);

        if (animationData == null)
        {
            GD.PrintErr("Failed to parse JSON.");
            return;
        }

        var animation = new Animation
        {
            Length = animationData.Count * FrameDuration,
            LoopMode = Animation.LoopModeEnum.Linear
        };
        var library = new AnimationLibrary();
        library.AddAnimation(AnimationName, animation);
        var parts = new Dictionary<string, string>
        {
            { "Hip", "Hip"},
            { "HipRight", "Hip/HipRight" },
            { "HipLeft", "Hip/HipLeft" },
            { "Chest", "Hip/Chest" },
            { "Head", "Hip/Chest/Head" },
            { "Torso", "Hip/Chest" },
            { "ShoulderRight", "Hip/Chest/ShoulderRight" },
            { "ArmRight", "Hip/Chest/ShoulderRight/ArmRight" },
            { "HandRight", "Hip/Chest/ShoulderRight/ArmRight/HandRight" },
            { "ShoulderLeft", "Hip/Chest/ShoulderLeft" },
            { "ArmLeft", "Hip/Chest/ShoulderLeft/ArmLeft" },
            { "HandLeft", "Hip/Chest/ShoulderLeft/ArmLeft/HandLeft" },
            { "LegRight", "Hip/HipRight/LegRight" },
            { "FootRight", "Hip/HipRight/LegRight/FootRight" },
            { "LegLeft", "Hip/HipLeft/LegLeft" },
            { "FootLeft", "Hip/HipLeft/LegLeft/FootLeft" }
        };

        foreach (var (frameStr, boneTransforms) in animationData)
        {
            if (!int.TryParse(frameStr, out int frameNum))
                continue;

            float time = (frameNum - 1) * FrameDuration;

            foreach (var (boneName, tf) in boneTransforms)
            {
                // string bonePath = Skeleton.GetPathTo(Skeleton.GetNode(parts[boneName]));
                var bonePath = parts[boneName];
                string posTrack = $"Skeleton/{bonePath}:position";
                string rotTrack = $"Skeleton/{bonePath}:rotation";

                // Add position track if not exists
                var posIdx = animation.FindTrack(posTrack, Animation.TrackType.Animation);
                if (posIdx == -1)
                    posIdx = animation.AddTrack(Animation.TrackType.Value);
                animation.TrackSetPath(posIdx, posTrack);
                animation.TrackInsertKey(posIdx, time, new Vector2(tf.X, tf.Y) * 100f);

                // Add rotation track if not exists
                var rotIdx = animation.FindTrack(rotTrack, Animation.TrackType.Animation);
                if (rotIdx == -1)
                    rotIdx = animation.AddTrack(Animation.TrackType.Value);
                animation.TrackSetPath(rotIdx, rotTrack);
                animation.TrackInsertKey(rotIdx, time, tf.Rotation);
            }
        }

        ResourceSaver.Save(library, $"res://resources/entities/player/{AnimationName}-lib.tres");
        var AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        AnimationPlayer.AddAnimationLibrary(AnimationName, library);
        // AnimationPlayer.Play(AnimationName);
    }

    private struct TransformFrame
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Rotation { get; set; }
    }

    private void AddBoneRotationKeyframes(Animation anim, string boneName, float angle1, float angle2)
    {
        string path = $"Skeleton:{boneName}:rotation_degrees";
        anim.AddTrack(Animation.TrackType.Value);
        int track = anim.GetTrackCount() - 1;
        anim.TrackSetPath(track, path);

        // Add keys at 0s, 0.3s, and 0.6s
        anim.TrackInsertKey(track, 0.0f, angle1);
        anim.TrackInsertKey(track, 0.3f, angle2);
        anim.TrackInsertKey(track, 0.6f, angle1); // loop
    }

    // Method to change the texture of a specific body part
    public void ChangeClothes(string part, string kindAndSet)
    {
        if (part == "weapon")
        {
            // Handle weapon separately if needed
            GD.Print($"Got to implement weapon first");
            return;
        }
        var parts = new Dictionary<string, string>
        {
            { "head", "Chest/Head/HeadSprite" },
            { "torso", "Chest/TorsoSprite" },
            { "shoulder-right", "Chest/ShoulderRight/ShoulderRightSprite" },
            { "hand-right", "Chest/ShoulderRight/ArmRight/HandRight/HandRightSprite" },
            { "shoulder-left", "Chest/ShoulderLeft/ShoulderLeftSprite" },
            { "hand-left", "Chest/ShoulderLeft/ArmLeft/HandLeft/HandLeftSprite" },
            { "leg-right", "HipRight/LegRightSprite" },
            { "foot-right", "HipRight/LegRight/FootRight/FootRightSprite" },
            { "leg-left", "HipLeft/LegLeftSprite" },
            { "foot-left", "HipLeft/LegLeft/FootLeft/FootLeftSprite" }
        };
        var sprite = GetNode<Sprite2D>($"Skeleton/Hip/{parts[part]}");
        sprite.Texture = GD.Load<Texture2D>($"res://resources/gear/{kindAndSet}/{part}.png");
    }

    public void RemoveSlot(string part)
    {
        var parts = new Dictionary<string, string>
        {
            { "head", "Chest/Head/HeadSprite" },
            { "torso", "Chest/TorsoSprite" },
            { "shoulder-right", "Chest/ShoulderRight/ShoulderRightSprite" },
            { "hand-right", "Chest/ShoulderRight/ArmRight/HandRight/HandRightSprite" },
            { "shoulder-left", "Chest/ShoulderLeft/ShoulderLeftSprite" },
            { "hand-left", "Chest/ShoulderLeft/ArmLeft/HandLeft/HandLeftSprite" },
            { "leg-right", "HipRight/LegRightSprite" },
            { "foot-right", "HipRight/LegRight/FootRight/FootRightSprite" },
            { "leg-left", "HipLeft/LegLeftSprite" },
            { "foot-left", "HipLeft/LegLeft/FootLeft/FootLeftSprite" }
        };
        var sprite = GetNode<Sprite2D>($"Skeleton/Hip/{parts[part]}");
        sprite.Texture = null;
    }

    private Dictionary<string, Dictionary<string, float>> LoadFrameData(string frameId)
    {
        // var fullData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, float>>>>(_frames);

        if (_frames.TryGetValue(frameId, out var frameData))
        {
            var convertedFrameData = new Dictionary<string, Dictionary<string, float>>();
            foreach (var kv in frameData)
            {
                convertedFrameData[kv.Key] = new Dictionary<string, float>
                {
                    { "x", kv.Value.x },
                    { "y", kv.Value.y },
                    { "rotation", kv.Value.rotation }
                };
            }
            return convertedFrameData;
        }

        return new Dictionary<string, Dictionary<string, float>>();
    }



    public void ApplyFrame(Dictionary<string, Dictionary<string, float>> frameData)
    {
        var parts = new Dictionary<string, string>
        {
            {"Hip", "Hip"},
            { "HipRight", "Hip/HipRight" },
            { "HipLeft", "Hip/HipLeft" },
            { "Chest", "Chest" },
            { "Head", "Chest/Head" },
            { "Torso", "Chest" },
            { "ShoulderRight", "Chest/ShoulderRight" },
            { "ArmRight", "Chest/ShoulderRight/ArmRight" },
            { "HandRight", "Chest/ShoulderRight/ArmRight/HandRight" },
            { "ShoulderLeft", "Chest/ShoulderLeft" },
            { "ArmLeft", "Chest/ShoulderLeft/ArmLeft" },
            { "HandLeft", "Chest/ShoulderLeft/ArmLeft/HandLeft" },
            { "LegRight", "HipRight/LegRight" },
            { "FootRight", "HipRight/LegRight/FootRight" },
            { "LegLeft", "HipLeft/LegLeft" },
            { "FootLeft", "HipLeft/LegLeft/FootLeft" }
        };
        foreach (var kv in frameData)
        {
            string boneName = kv.Key;
            var boneData = kv.Value;

            Node boneNode = GetNodeOrNull("Skeleton/" + parts[boneName]);
            if (boneNode is not Bone2D bone) continue;

            float x = boneData.TryGetValue("x", out var xv) ? xv : 0f;
            float y = boneData.TryGetValue("y", out var yv) ? yv : 0f;
            float rotation = boneData.TryGetValue("rotation", out var rv) ? rv : 0f;

            bone.Position = new Vector2(x, y) * 100f;
            bone.RotationDegrees = rotation;
        }
    }
}
