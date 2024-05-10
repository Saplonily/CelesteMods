local IHPHKDialogEntity =
{
    name = "CNY2024Helper/IHPHKDialogEntity",
    placements =
    {
        {
            name = "CNY2024Helper/IHPHKDialogEntity",
            data = {
                depth = 100000,
                name = "ChineseNewYear2024Helper_OPTIMIZE2_IHPHKDIALOGENTITY_EXAMPLE_NAME",
                spriteName = "glider",
                spriteAnim = "idle",
                spriteScaleX = 1,
                spriteScaleY = 1,
                spriteOutline = true,
                dialogId = "ChineseNewYear2024Helper_OPTIMIZE2_IHPHKDIALOGENTITY_EXAMPLE_DIALOG",
                randomPlay = false,
                editorTexture = "objects/glider/idle0",
            },
        },
    },
};

function IHPHKDialogEntity.texture(room, entity)
    local editorTexture = entity.editorTexture or "objects/glider/idle0";

    return editorTexture;
end

return IHPHKDialogEntity;