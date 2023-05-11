import React, { useEffect, useState } from 'react';
import getBackgroundStyle from './getBackgroundStyle';
import './App.css';

const maxUpdateTime = 60 * 1000;

export default function App() {
    useEffect(() => {
        Object.assign(document.body.style, getBackgroundStyle());

        fetchState();
        const intervalId = setInterval(async () => {
            await fetchState();
        }, 1000);
        return () => {
            clearInterval(intervalId);
        };
    }, []);

    const [state, setState] = useState({
        isOn: null,
        onUntil: null,
        lastUpdate: null,
        temperature: {
            hasValue: false,
            value: null,
            type: 'Equal',
        },
    });

    const onTimeRef = React.createRef();

    const nowMillis = new Date().getTime();
    const millisSinceLastUpdate = state.lastUpdate ? nowMillis - state.lastUpdate.getTime() : -1;

    let statusText;
    let statusColor;
    if (state.isOn === null) {
        statusText = "Status ist unbekannt.";
        statusColor = "black";
        document.title = "Wasserpumpe";
    }
    else if (state.isOn === false) {
        statusText = "Pumpe ist ausgeschalten.";
        statusColor = "blue";
        document.title = "Wasserpumpe aus";
    }
    else if (millisSinceLastUpdate < 0 ||
        millisSinceLastUpdate > maxUpdateTime) {
        statusText = "Pumpe ist wahrscheinlich ausgeschalten.";
        statusColor = "blue";
        document.title = "Wasserpumpe aus";
    }
    else if (state.isOn === true) {
        statusText = "Pumpe ist eingeschalten.";
        statusColor = "green";
        document.title = "Wasserpumpe an";
    }
    else {
        statusText = "Bug: " + state.isOn;
        statusColor = "red";
        document.title = "Wasserpumpe";
    }

    let updateText;
    if (millisSinceLastUpdate < 0) {
        updateText = "Kein Update bekannt.";
    }
    else {
        updateText = "Letztes Update vor: " + timeToString(millisSinceLastUpdate);
    }

    let tempText;
    if (!state.temperature.hasValue) {
        tempText = "Kein Temperatur bekannt.";
    }
    else if (state.temperature.type === 0) {
        tempText = `Temperatur: ${state.temperature.value.toFixed(1)} \u00B0C`;
    }
    else if (state.temperature.type === 1) {
        tempText = `Temperatur: < ${state.temperature.value.toFixed(1)} \u00B0C`;
    }
    else if (state.temperature.type === 2) {
        tempText = `Temperatur: > ${state.temperature.value.toFixed(1)} \u00B0C`;
    }

    let countdownText;
    let countdownDisplay;
    const remainingOnMillis = state.isOn && state.onUntil ? state.onUntil.getTime() - nowMillis : -1;
    if (remainingOnMillis > 0) {
        countdownText = timeToString(remainingOnMillis);
        countdownDisplay = 'block';
    }
    else {
        countdownText = "";
        countdownDisplay = 'none';
    }

    function setResult(result) {
        if (result) {
            const nowMillis = new Date().getTime();
            setState({
                isOn: result.isOn,
                onUntil: result.remainingOnMillis && new Date(nowMillis + result.remainingOnMillis),
                lastUpdate: result.lastUpdateMillisAgo && new Date(nowMillis - result.lastUpdateMillisAgo),
                temperature: result.temperature,
            });
        } else {
            setState({
                isOn: null,
                onUntil: null,
                lastUpdate: null,
                temperature: {
                    hasValue: false,
                    value: null,
                    type: 'Equal',
                },
            });
        }
    }

    async function fetchState() {
        const response = await fetch('/api/waterpump/state');
        if (response.ok) {
            setResult(await response.json());
        } else {
            setResult(null);
        }
    }

    function timeToString(distance) {
        // Time calculations for days, hours, minutes and seconds
        var days = Math.floor(distance / (1000 * 60 * 60 * 24));
        var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        var seconds = Math.floor((distance % (1000 * 60)) / 1000);

        var timeText = "";
        if (days > 0) timeText += days + "d ";
        if (days > 0 || hours > 0) timeText += hours + "h ";
        if (days > 0 || hours > 0 || minutes > 0) timeText += minutes + "m ";
        timeText += seconds + "s ";

        return timeText;
    }

    async function turnOn() {
        const turnOnMinutes = onTimeRef.current.value;
        if (turnOnMinutes > 0) {
            await fetch('/api/waterpump/on?time=' + turnOnMinutes);
            await fetchState();
        }
    }

    async function turnOff() {
        await fetch('/api/waterpump/off');
        await fetchState();
    }

    return (
        <div className="app-container">
            <h1 className="status" style={{ color: statusColor }}>{statusText}</h1>
            <h2 className="update">{updateText}</h2>
            <h2 className="temp">{tempText}</h2>
            <h1 className="countdown" style={{ display: countdownDisplay }}>{countdownText}</h1>
            <br />
            <br />
            <br />
            <div className="form">
                Zeit (Minuten):
                <div>
                    <input ref={onTimeRef} type="number" defaultValue="5" className="time form-control"></input>
                    <br />
                    <div>
                        <input type="button" value="Einschalten" className="btn bg-primary text-light turnonoff" onClick={turnOn}></input>
                        <input type="button" value="Abschalten" className="btn bg-secondary text-light turnonoff" onClick={turnOff}></input>
                    </div>
                </div>
            </div>
        </div>
    );
}
