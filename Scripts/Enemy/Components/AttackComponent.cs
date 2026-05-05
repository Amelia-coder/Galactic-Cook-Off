using Godot;
using Godot.Collections;
using System;

public partial class AttackComponent
{
    private Dictionary<Type, AttackStrategy> _attackStrategies = new();

    public void Initialize(CharacterBody3D body, StaminaComponent stamina)
    {
        _body = body;
        _stamina = stamina;
    }
}

