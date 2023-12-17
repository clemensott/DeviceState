/**
   BasicHTTPClient.ino

    Created on: 24.05.2015

*/

#include <Arduino.h>

#include <ESP8266WiFi.h>
#include <ESP8266WiFiMulti.h>

#include <ESP8266HTTPClient.h>

#include <WiFiClient.h>
#include "env.h"

const int relayPin = 5;
const int tempPin = A0;
const long maxErrorCount = 15;

long errorCount;
long startRoundMillis;

ESP8266WiFiMulti WiFiMulti;

void setup() {
  pinMode(relayPin, OUTPUT);
  pinMode(tempPin, INPUT);

  digitalWrite(relayPin, LOW);

  Serial.begin(115200);
  // Serial.setDebugOutput(true);

  Serial.println();
  Serial.println();
  Serial.println();

  for (uint8_t t = 4; t > 0; t--) {
    Serial.printf("[SETUP] WAIT %d...\n", t);
    Serial.flush();
    delay(1000);
  }

  WiFi.mode(WIFI_STA);
  WiFiMulti.addAP(STASSID, STAPSK);

  errorCount = 0;
}

void loop() {
  startRoundMillis = millis();

  // wait for WiFi connection
  if ((WiFiMulti.run() == WL_CONNECTED)) {

    WiFiClient client;
    HTTPClient http;

    String url = "http://";
    // simplist way to implement call server via IP and Hostname
    if (errorCount % 2 == 0) url += IP;
    else url += HOSTNAME;
    url += "/api/device/ison?id=";
    url += DEVICE_ID;
    url += "&errors=";
    url += String(errorCount, DEC);
    url += "&state=";
    url += String(digitalRead(relayPin), DEC);
    url += "&value=";
    url += String(analogRead(tempPin), DEC);
    url += "&maxWaitMillis=10000";

    Serial.println("[HTTP] begin...");
    Serial.println(url);

    if (http.begin(client, url)) { // HTTP
      Serial.print("[HTTP] GET...\n");
      http.setTimeout(20000);
      // start connection and send HTTP header
      int httpCode = http.GET();

      // httpCode will be negative on error
      if (httpCode > 0) {
        // HTTP header has been send and Server response header has been handled
        Serial.printf("[HTTP] GET... code: %d\n", httpCode);

        // file found at server
        if (httpCode == HTTP_CODE_OK || httpCode == HTTP_CODE_MOVED_PERMANENTLY) {
          String payload = http.getString();
          Serial.println(payload);

          if (payload == "True") {
            digitalWrite(relayPin, HIGH);
            errorCount = 0;
          }
          else if (payload == "False") {
            digitalWrite(relayPin, LOW);
            errorCount = 0;
          }
          else errorCount++;

        }
      } else {
        Serial.printf("[HTTP] GET... failed, error: %s\n", http.errorToString(httpCode).c_str());
        errorCount++;
      }

      http.end();
    } else {
      Serial.printf("[HTTP} Unable to connect\n");
      errorCount++;
    }
  } else {
    Serial.printf("[WIFI} Not connected: %d\n", WiFiMulti.run());
    errorCount++;
  }

  if (errorCount >= maxErrorCount) {
    digitalWrite(relayPin, LOW);
  }

  Serial.print("Pump: ");
  Serial.println(digitalRead(relayPin));

  if (millis() - startRoundMillis < 300) {
    delay(1000);
  }
}
