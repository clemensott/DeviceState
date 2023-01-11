import React, { Component } from 'react';
import "./App.css"

export default class App extends Component {

    constructor(props) {
        super(props);

        this.onTimeRef = React.createRef();

        this.maxUpdateTime = 60 * 1000;
        this.state = {
            isOn: null,
            onUntil: null,
            lastUpdate: null,
            temperature: {
                hasValue: false,
                value: null,
                type: 'Equal',
            }
        };
        this.turnOn = this.turnOn.bind(this);
        this.turnOff = this.turnOff.bind(this);
    }

    static timeToString(distance) {
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

    async turnOn() {
        const turnOnMinutes = this.onTimeRef.current.value;
        if (turnOnMinutes > 0) {
            await fetch('/api/waterpump/on?time=' + turnOnMinutes);
            await this.fetchState();
        }
    }

    async turnOff() {
        await fetch('/api/waterpump/off');
        await this.fetchState();
    }

    render() {
        let statusText, statusColor, updateText, tempText, countdownText, countdownDisplay;

        const nowMillis = new Date().getTime();
        const millisSinceLastUpdate = this.state.lastUpdate ? nowMillis - this.state.lastUpdate.getTime() : -1;

        if (this.state.isOn === null) {
            statusText = "Status ist unbekannt.";
            statusColor = "black";
            document.title = "Wasserpume";
        }
        else if (this.state.isOn === false) {
            statusText = "Pumpe ist ausgeschalten.";
            statusColor = "blue";
            document.title = "Wasserpume aus";
        }
        else if (millisSinceLastUpdate < 0 ||
            millisSinceLastUpdate > this.maxUpdateTime) {
            statusText = "Pumpe ist wahrscheinlich ausgeschalten.";
            statusColor = "blue";
            document.title = "Wasserpume aus";
        }
        else if (this.state.isOn === true) {
            statusText = "Pumpe ist eingeschalten.";
            statusColor = "green";
            document.title = "Wasserpume an";
        }
        else {
            statusText = "Bug: " + this.state.isOn;
            statusColor = "red";
            document.title = "Wasserpume";
        }

        if (millisSinceLastUpdate < 0) {
            updateText = "Kein Update bekannt.";
        }
        else {
            updateText = "Letztes Update vor: " + App.timeToString(millisSinceLastUpdate);
        }

        if (!this.state.temperature.hasValue) {
            tempText = "Kein Temperatur bekannt.";
        }
        else if (this.state.temperature.type === 0) {
            tempText = `Temperatur: ${this.state.temperature.value.toFixed(1)} \u00B0C`;
        }
        else if (this.state.temperature.type === 1) {
            tempText = `Temperatur: < ${this.state.temperature.value.toFixed(1)} \u00B0C`;
        }
        else if (this.state.temperature.type === 2) {
            tempText = `Temperatur: > ${this.state.temperature.value.toFixed(1)} \u00B0C`;
        }

        const remainingOnMillis = this.state.isOn && this.state.onUntil ? this.state.onUntil.getTime() - nowMillis : -1;
        if (remainingOnMillis > 0) {
            countdownText = App.timeToString(remainingOnMillis);
            countdownDisplay = 'block';
        }
        else {
            countdownText = "";
            countdownDisplay = 'none';
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
                        <input ref={this.onTimeRef} type="number" defaultValue="5" className="time form-control"></input>
                        <br />
                        <div>
                            <input type="button" value="Einschalten" className="btn bg-primary text-light turnonoff" onClick={this.turnOn}></input>
                            <input type="button" value="Abschalten" className="btn bg-secondary text-light turnonoff" onClick={this.turnOff}></input>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    setResult(result) {
        if (result) {
            const nowMillis = new Date().getTime();
            this.setState({
                isOn: result.isOn,
                onUntil: result.remainingOnMillis && new Date(nowMillis + result.remainingOnMillis),
                lastUpdate: result.lastUpdateMillisAgo && new Date(nowMillis - result.lastUpdateMillisAgo),
                temperature: result.temperature,
            })
        } else {
            this.setState({
                isOn: null,
                onUntil: null,
                lastUpdate: null,
                temperature: {
                    hasValue: false,
                    value: null,
                    type: 'Equal',
                }
            });
        }
    }

    async fetchState() {
        const response = await fetch('/api/waterpump/state');
        if (response.status === 200) {
            this.setResult(await response.json());
        } else {
            this.setResult(null);
        }
    }

    async componentDidMount() {
        await this.fetchState();

        setInterval(async () => {
            await this.fetchState();
        }, 1000);
    }
}
