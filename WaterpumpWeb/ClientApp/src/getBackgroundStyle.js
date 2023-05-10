function getRandomColorPart(maxValue = 128) {
    return 255 - Math.random() * maxValue;
}

export default function getBackgroundStyle() {
    const rotation = Math.random();
    const color1 = `rgb(${getRandomColorPart()},${getRandomColorPart()},${getRandomColorPart()})`;
    const color2 = `rgb(${getRandomColorPart()},${getRandomColorPart()},${getRandomColorPart()})`;

    return {
        background: `linear-gradient(${rotation}turn, ${color1}, ${color2})`,
    };
}
