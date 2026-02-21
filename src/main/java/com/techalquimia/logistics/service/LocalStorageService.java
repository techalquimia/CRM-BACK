package com.techalquimia.logistics.service;

import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.stereotype.Service;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.UUID;

/**
 * Almacenamiento local cuando GCP no está configurado.
 */
@Service
@ConditionalOnProperty(name = "gcp.enabled", havingValue = "false", matchIfMissing = true)
@Slf4j
public class LocalStorageService implements StorageService {

    @Value("${app.storage.local-path:./uploads}")
    private String localPath;

    @Override
    public String uploadImage(MultipartFile file, String folder, UUID userId) throws IOException {
        Path baseDir = Paths.get(localPath, folder, userId.toString());
        Files.createDirectories(baseDir);

        String originalFilename = file.getOriginalFilename();
        String extension = originalFilename != null && originalFilename.contains(".")
                ? originalFilename.substring(originalFilename.lastIndexOf("."))
                : ".jpg";
        String fileName = UUID.randomUUID() + extension;
        Path targetPath = baseDir.resolve(fileName);

        Files.write(targetPath, file.getBytes());

        String url = "file://" + targetPath.toAbsolutePath().normalize();
        log.debug("Imagen guardada localmente: {}", url);
        return url;
    }
}
