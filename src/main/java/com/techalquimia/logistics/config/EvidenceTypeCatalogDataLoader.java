package com.techalquimia.logistics.config;

import com.techalquimia.logistics.entity.EvidenceType;
import com.techalquimia.logistics.entity.EvidenceTypeCatalog;
import com.techalquimia.logistics.repository.EvidenceTypeCatalogRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.boot.CommandLineRunner;
import org.springframework.stereotype.Component;

import java.util.List;

/**
 * Carga inicial del catálogo de tipos de evidencia.
 */
@Component
@RequiredArgsConstructor
@Slf4j
public class EvidenceTypeCatalogDataLoader implements CommandLineRunner {

    private final EvidenceTypeCatalogRepository repository;

    @Override
    public void run(String... args) {
        List<EvidenceTypeCatalog> catalog = List.of(
                EvidenceTypeCatalog.builder()
                        .code(EvidenceType.WEIGHT_TICKET.name())
                        .name("Ticket de báscula")
                        .description("Ticket de báscula (Requiere OCR para peso total, tara y peso neto)")
                        .requiresOcr(true)
                        .build(),
                EvidenceTypeCatalog.builder()
                        .code(EvidenceType.TRUCK_PHOTO.name())
                        .name("Foto del camión")
                        .description("Foto del camión (Solo almacenamiento)")
                        .requiresOcr(false)
                        .build(),
                EvidenceTypeCatalog.builder()
                        .code(EvidenceType.SEAL_PHOTO.name())
                        .name("Foto del sello/precinto")
                        .description("Foto del sello o precinto")
                        .requiresOcr(false)
                        .build(),
                EvidenceTypeCatalog.builder()
                        .code(EvidenceType.DELIVERY_PROOF.name())
                        .name("Prueba de entrega firmada")
                        .description("Prueba de entrega firmada")
                        .requiresOcr(false)
                        .build(),
                EvidenceTypeCatalog.builder()
                        .code(EvidenceType.INCIDENT_REPORT.name())
                        .name("Reporte de incidente")
                        .description("Reporte de incidente")
                        .requiresOcr(false)
                        .build()
        );

        for (EvidenceTypeCatalog item : catalog) {
            if (!repository.existsByCode(item.getCode())) {
                repository.save(item);
                log.info("Tipo de evidencia creado: {}", item.getCode());
            }
        }
    }
}
