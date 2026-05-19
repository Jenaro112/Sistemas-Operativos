document.addEventListener("DOMContentLoaded", async () => {
  const codeBlocks = document.querySelectorAll(".code-block");

  for (const block of codeBlocks) {
    const src = block.dataset.src;

    if (!src) continue;

    try {
      const response = await fetch(src);

      if (!response.ok) {
        throw new Error(`No se pudo cargar: ${src}`);
      }

      const code = await response.text();

      block.textContent = code;

      Prism.highlightElement(block);
    } catch (error) {
      console.error(error);

      block.textContent = "Error al cargar el archivo de código.";
    }
  }
});
