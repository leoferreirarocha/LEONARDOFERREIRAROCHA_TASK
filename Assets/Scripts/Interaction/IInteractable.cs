namespace LeonardoTask.Interaction
{
    /// <summary>
    /// Defines the contract required by any object that can be
    /// interacted with by the player.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Gets the text displayed beside the interaction key.
        /// </summary>
        string InteractionLabel { get; }

        /// <summary>
        /// Gets whether this object can currently be interacted with.
        /// </summary>
        bool CanInteract { get; }

        /// <summary>
        /// Performs the object's interaction behavior.
        /// </summary>
        void Interact(PlayerInteractor2D interactor);
    }
}