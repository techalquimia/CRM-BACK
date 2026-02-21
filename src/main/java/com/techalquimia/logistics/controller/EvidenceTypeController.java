package com.techalquimia.logistics.controller;

import com.techalquimia.logistics.dto.EvidenceTypeCatalogResponse;
import com.techalquimia.logistics.repository.EvidenceTypeCatalogRepository;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.tags.Tag;
import lombok.RequiredArgsConstructor;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;

@RestController
@RequestMapping("/api/v1/evidence-types")
@RequiredArgsConstructor
@Tag(name = "Catálogo de tipos de evidencia", description = "Lista de tipos de evidencia disponibles")
public class EvidenceTypeController {

    private final EvidenceTypeCatalogRepository repository;

    @GetMapping
    @Operation(summary = "Listar tipos de evidencia", description = "Obtiene el catálogo de tipos de evidencia. Público.")
    public List<EvidenceTypeCatalogResponse> listEvidenceTypes() {
        return repository.findAll().stream()
                .map(e -> EvidenceTypeCatalogResponse.builder()
                        .id(e.getId())
                        .code(e.getCode())
                        .name(e.getName())
                        .description(e.getDescription())
                        .requiresOcr(e.getRequiresOcr())
                        .build())
                .toList();
    }
}
