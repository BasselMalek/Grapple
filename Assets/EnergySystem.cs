using System;
using UnityEngine;

public class EnergySystem
{

    public event EventHandler OnEnergyChanged;
    public event EventHandler OnEnergyMaxChanged;
    public event EventHandler OnEnergyDamaged;
    public event EventHandler OnEnergyRegened;
    public event EventHandler OnEnergyDepleted;

    private float energyMax;
    private float energy;
    private float depletionRate;
    private float regenRate;
    private bool isAutoDepleting;

    public EnergySystem(float energyMax, float depletionRate = 1f, float regenRate = 0f)
    {
        this.energyMax = energyMax;
        energy = energyMax;
        this.depletionRate = depletionRate;
        this.regenRate = regenRate;
        isAutoDepleting = true;
    }

    public float GetEnergy()
    {
        return energy;
    }

    public float GetEnergyMax()
    {
        return energyMax;
    }

    public float GetEnergyNormalized()
    {
        return energy / energyMax;
    }

    public float GetDepletionRate()
    {
        return depletionRate;
    }

    public float GetRegenRate()
    {
        return regenRate;
    }

    public bool IsAutoDepleting()
    {
        return isAutoDepleting;
    }

    public void Update(float deltaTime)
    {
        if (isAutoDepleting)
        {
            UseEnergy(depletionRate * deltaTime);
        }

        if (regenRate > 0)
        {
            RegenEnergy(regenRate * deltaTime);
        }
    }

    public void EnergyDamage(float amount)
    {
        energy -= amount;
        if (energy < 0)
        {
            energy = 0;
        }
        OnEnergyChanged?.Invoke(this, EventArgs.Empty);
        OnEnergyDamaged?.Invoke(this, EventArgs.Empty);

        if (energy <= 0)
        {
            Deplete();
        }
    }

    public void Deplete()
    {
        OnEnergyDepleted?.Invoke(this, EventArgs.Empty);
    }

    public bool IsDepleted()
    {
        return energy <= 0;
    }

    public void RegenEnergy(float amount)
    {
        energy += amount;
        if (energy > energyMax)
        {
            energy = energyMax;
        }
        OnEnergyChanged?.Invoke(this, EventArgs.Empty);
        OnEnergyRegened?.Invoke(this, EventArgs.Empty);
    }

    public void RegenFull()
    {
        energy = energyMax;
        OnEnergyChanged?.Invoke(this, EventArgs.Empty);
        OnEnergyRegened?.Invoke(this, EventArgs.Empty);
    }

    public void SetEnergyMax(float energyMax, bool fullEnergy)
    {
        this.energyMax = energyMax;
        if (fullEnergy) energy = energyMax;
        OnEnergyMaxChanged?.Invoke(this, EventArgs.Empty);
        OnEnergyChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetEnergy(float energy)
    {
        if (energy > energyMax)
        {
            energy = energyMax;
        }
        if (energy < 0)
        {
            energy = 0;
        }
        this.energy = energy;
        OnEnergyChanged?.Invoke(this, EventArgs.Empty);

        if (energy <= 0)
        {
            Deplete();
        }
    }

    public void UseEnergy(float amount)
    {
        energy -= amount;
        if (energy < 0)
        {
            energy = 0;
        }
        OnEnergyChanged?.Invoke(this, EventArgs.Empty);

        if (energy <= 0)
        {
            Deplete();
        }
    }

    public void SetDepletionRate(float depletionRate)
    {
        this.depletionRate = depletionRate;
    }

    public void SetRegenRate(float regenRate)
    {
        this.regenRate = regenRate;
    }

    public void SetAutoDepleting(bool isAutoDepleting)
    {
        this.isAutoDepleting = isAutoDepleting;
    }

    public void HandlePickup()
    {
        this.RegenEnergy(10);
    }

    public static bool TryGetEnergySystem(GameObject getEnergySystemGameObject, out EnergySystem energySystem, bool logErrors = false)
    {
        energySystem = null;

        if (getEnergySystemGameObject != null)
        {
            if (getEnergySystemGameObject.TryGetComponent(out IGetEnergySystem getEnergySystem))
            {
                energySystem = getEnergySystem.GetEnergySystem();
                if (energySystem != null)
                {
                    return true;
                }
                else
                {
                    if (logErrors)
                    {
                        Debug.LogError($"Got EnergySystem from object but energySystem is null! Should it have been created? Maybe you have an issue with the order of operations.");
                    }
                    return false;
                }
            }
            else
            {
                if (logErrors)
                {
                    Debug.LogError($"Referenced Game Object '{getEnergySystemGameObject}' does not have a script that implements IGetEnergySystem!");
                }
                return false;
            }
        }
        else
        {
            if (logErrors)
            {
                Debug.LogError($"You need to assign the field 'getEnergySystemGameObject'!");
            }
            return false;
        }
    }
}

public interface IGetEnergySystem
{
    EnergySystem GetEnergySystem();
}