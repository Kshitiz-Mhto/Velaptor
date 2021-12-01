﻿/*Info:
This is an example of alpha blending.

Src: https://stackoverflow.com/questions/746899/how-to-calculate-an-rgb-colour-by-specifying-an-alpha-blending-amount
*/
#version 450 core

// TODO: Refactor in var prefix from 'v_' to 'pass_'
in vec2 pass_textureCoord;
in vec4 pass_tintColor;

out vec4 o_OutputColor;

uniform sampler2D texture0;//For regular texture rendering

/*
    Maps the given value from one range to another.

    @param value The value to map.
    @param fromStart The from starting range value.
    @param fromStop The from ending range value.
    @param toStart The to starting range value.
    @param toStop The to ending range value.
*/
float mapValue(float value, float fromStart, float fromStop, float toStart, float toStop)
{
    return toStart + ((toStop - toStart) * ((value - fromStart) / (fromStop - fromStart)));
}

/*
    Converts the given color in pixel units to a color with
    NDC(Normalized Device Coordinate) units.
*/
vec4 toNDCColor(vec4 pixelColor)
{
    float red = mapValue(pixelColor.r, 0.0, 255.0, 0.0, 1.0);
    float green = mapValue(pixelColor.g, 0.0, 255.0, 0.0, 1.0);
    float blue = mapValue(pixelColor.b, 0.0, 255.0, 0.0, 1.0);
    float alpha = mapValue(pixelColor.a, 0.0, 255.0, 0.0, 1.0);

    return vec4(red, green, blue, alpha);
}

void main ()
{
    vec4 ndcColor = toNDCColor(pass_tintColor);

	o_OutputColor = texture(texture0, pass_textureCoord) * ndcColor;
}
