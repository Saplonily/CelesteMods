local field = {}

field.name = "EMField/UniformRectangleEField"
field.fillColor = { 0.7294, 0.8745, 0.9568, 0.2 }
field.borderColor = { 1.0, 1.0, 1.0 }
field.nodeLimits = { 1, 1 }
field.nodeLineRenderType = "line"
field.placements = {
    name = "normal",
    data = {
        width = 8,
        height = 8,
        intensity = 30
    }
}

return field