// Grid Canvas JavaScript Module for efficient rendering

export function initCanvas(canvas, gridWidth, gridHeight, cellSize) {
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    
    // Clear canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    // Draw initial grid lines (optional, can be removed for better performance)
    ctx.strokeStyle = '#e0e0e0';
    ctx.lineWidth = 0.5;
    
    // Vertical lines
    for (let x = 0; x <= gridWidth; x++) {
        ctx.beginPath();
        ctx.moveTo(x * cellSize, 0);
        ctx.lineTo(x * cellSize, gridHeight * cellSize);
        ctx.stroke();
    }
    
    // Horizontal lines
    for (let y = 0; y <= gridHeight; y++) {
        ctx.beginPath();
        ctx.moveTo(0, y * cellSize);
        ctx.lineTo(gridWidth * cellSize, y * cellSize);
        ctx.stroke();
    }
}

export function renderGrid(canvas, flatGrid, colorMap, gridWidth, gridHeight, cellSize) {
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    
    // Use image data for faster rendering
    const imageData = ctx.createImageData(gridWidth * cellSize, gridHeight * cellSize);
    const data = imageData.data;
    
    // Convert color map to RGB
    const colorCache = {};
    for (const [key, color] of Object.entries(colorMap)) {
        colorCache[key] = hexToRgb(color);
    }
    
    // Fill pixels
    for (let y = 0; y < gridHeight; y++) {
        for (let x = 0; x < gridWidth; x++) {
            const cellValue = flatGrid[y * gridWidth + x];
            const color = colorCache[cellValue] || { r: 255, g: 255, b: 255 };
            
            // Fill the cell
            for (let py = 0; py < cellSize; py++) {
                for (let px = 0; px < cellSize; px++) {
                    const pixelX = x * cellSize + px;
                    const pixelY = y * cellSize + py;
                    const index = (pixelY * gridWidth * cellSize + pixelX) * 4;
                    
                    data[index] = color.r;
                    data[index + 1] = color.g;
                    data[index + 2] = color.b;
                    data[index + 3] = 255; // Alpha
                }
            }
        }
    }
    
    ctx.putImageData(imageData, 0, 0);
    
    // Draw grid lines on top (optional)
    drawGridLines(ctx, gridWidth, gridHeight, cellSize);
}

function drawGridLines(ctx, gridWidth, gridHeight, cellSize) {
    ctx.strokeStyle = '#e0e0e0';
    ctx.lineWidth = 0.5;
    
    // Vertical lines
    for (let x = 0; x <= gridWidth; x++) {
        ctx.beginPath();
        ctx.moveTo(x * cellSize, 0);
        ctx.lineTo(x * cellSize, gridHeight * cellSize);
        ctx.stroke();
    }
    
    // Horizontal lines
    for (let y = 0; y <= gridHeight; y++) {
        ctx.beginPath();
        ctx.moveTo(0, y * cellSize);
        ctx.lineTo(gridWidth * cellSize, y * cellSize);
        ctx.stroke();
    }
}

export function getCellFromClick(canvas, offsetX, offsetY, cellSize) {
    if (!canvas) return null;
    
    const x = Math.floor(offsetX / cellSize);
    const y = Math.floor(offsetY / cellSize);
    
    return [x, y];
}

function hexToRgb(hex) {
    // Remove # if present
    hex = hex.replace('#', '');
    
    // Handle CSS color names and special cases
    if (hex === 'white') return { r: 255, g: 255, b: 255 };
    if (hex === 'black') return { r: 0, g: 0, b: 0 };
    
    // Handle CSS variables (var(--...))
    if (hex.startsWith('var(')) {
        // Get computed color from CSS variable
        const root = document.documentElement;
        const style = getComputedStyle(root);
        const varName = hex.match(/var\((--[^)]+)\)/)?.[1];
        if (varName) {
            const color = style.getPropertyValue(varName).trim();
            if (color.startsWith('#')) {
                hex = color.substring(1);
            } else if (color.startsWith('rgb')) {
                const matches = color.match(/\d+/g);
                if (matches && matches.length >= 3) {
                    return {
                        r: parseInt(matches[0]),
                        g: parseInt(matches[1]),
                        b: parseInt(matches[2])
                    };
                }
            }
        }
    }
    
    // Parse hex color
    const r = parseInt(hex.substring(0, 2), 16);
    const g = parseInt(hex.substring(2, 4), 16);
    const b = parseInt(hex.substring(4, 6), 16);
    
    return { r: r || 0, g: g || 0, b: b || 0 };
}
