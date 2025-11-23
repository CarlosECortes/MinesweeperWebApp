function playClickEffect(event) {
    const button = event.target;

    // Create the effect
    const burst = document.createElement("div");
    burst.classList.add("starburst");

    // Position exactly over the clicked tile
    const rect = button.getBoundingClientRect();
    burst.style.left = rect.left + "px";
    burst.style.top = rect.top + "px";
    burst.style.position = "fixed";

    document.body.appendChild(burst);

    // Remove it after animation completes
    setTimeout(() => {
        burst.remove();
    }, 400);
}
