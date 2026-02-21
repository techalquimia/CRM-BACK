package com.techalquimia.logistics.service;

import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.util.UUID;

public interface StorageService {

    String uploadImage(MultipartFile file, String folder, UUID userId) throws IOException;
}
